using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class ResultPresenter : MonoBehaviour
{
    private const string VisibleClass = "result-screen--visible";

    private UIDocument document;
    private VisualElement resultScreen;

    private Label titleLabel;
    private Label distanceValueLabel;
    private Label maxHeightValueLabel;
    private Label flightTimeValueLabel;
    private Label rotationsValueLabel;
    private Label penaltyValueLabel;
    private Label finalScoreValueLabel;
    private Label resetHintLabel;

    private IVisualElementScheduledItem pendingShow;
    private bool initialized;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        InitializeIfNeeded();
        HideImmediate();
    }

    public void ShowResult(ResultData data)
    {
        if (!InitializeIfNeeded()) {
            return;
        }

        titleLabel.text = "RESULT";
        distanceValueLabel.text = $"{data.Distance:F2} m";
        maxHeightValueLabel.text = $"{data.MaxHeight:F2} m";
        flightTimeValueLabel.text = $"{data.FlightTime:F2} s";
        rotationsValueLabel.text = $"{data.Rotations:F1}";
        penaltyValueLabel.text = $"-{data.PenaltyPoints:N0} pt";
        finalScoreValueLabel.text = $"{data.FinalScore:N0} pt";
        resetHintLabel.text = "R : RESET";

        Show();
    }

    public void ShowMiss()
    {
        if (!InitializeIfNeeded()) {
            return;
        }

        titleLabel.text = "MISS!!";
        distanceValueLabel.text = "--";
        maxHeightValueLabel.text = "--";
        flightTimeValueLabel.text = "--";
        rotationsValueLabel.text = "--";
        penaltyValueLabel.text = "--";
        finalScoreValueLabel.text = "0 pt";
        resetHintLabel.text = "R : RESET";

        Show();
    }

    public void HideImmediate()
    {
        if (!InitializeIfNeeded()) {
            return;
        }

        pendingShow?.Pause();
        pendingShow = null;

        resultScreen.RemoveFromClassList(VisibleClass);
        resultScreen.style.display = DisplayStyle.None;
    }

    private void Show()
    {
        pendingShow?.Pause();

        // display:noneのままではTransitionが評価されないため、
        // 一度非表示クラスの状態でレイアウトへ戻し、次の更新で表示クラスを付ける。
        resultScreen.RemoveFromClassList(VisibleClass);
        resultScreen.style.display = DisplayStyle.Flex;

        pendingShow = resultScreen.schedule.Execute(() =>
        {
            resultScreen.AddToClassList(VisibleClass);
            pendingShow = null;
        }).StartingIn(16);
    }

    private bool InitializeIfNeeded()
    {
        if (initialized) {
            return true;
        }

        if (document == null) {
            document = GetComponent<UIDocument>();
        }

        VisualElement root = document != null
            ? document.rootVisualElement
            : null;

        if (root == null) {
            Debug.LogError("ResultPresenter: UIDocumentのVisual Treeを取得できません。", this);
            return false;
        }

        resultScreen = root.Q<VisualElement>("result-screen");
        titleLabel = root.Q<Label>("title");
        distanceValueLabel = root.Q<Label>("distance-value");
        maxHeightValueLabel = root.Q<Label>("max-height-value");
        flightTimeValueLabel = root.Q<Label>("flight-time-value");
        rotationsValueLabel = root.Q<Label>("rotations-value");
        penaltyValueLabel = root.Q<Label>("penalty-value");
        finalScoreValueLabel = root.Q<Label>("final-score-value");
        resetHintLabel = root.Q<Label>("reset-hint");

        if (resultScreen == null ||
            titleLabel == null ||
            distanceValueLabel == null ||
            maxHeightValueLabel == null ||
            flightTimeValueLabel == null ||
            rotationsValueLabel == null ||
            penaltyValueLabel == null ||
            finalScoreValueLabel == null ||
            resetHintLabel == null) {
            Debug.LogError(
                "ResultPresenter: ResultView.uxml内の必須要素が不足しています。" +
                "name属性を変更していないか確認してください。",
                this
            );
            return false;
        }

        initialized = true;
        return true;
    }
}
