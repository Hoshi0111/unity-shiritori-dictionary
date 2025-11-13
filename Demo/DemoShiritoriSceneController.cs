using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shiritori.Dictionary;

public class DemoShiritoriSceneController : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField ReadingInput;
    public Button CheckButton;
    public TMP_Dropdown AllowProperNounDropdown;
    public TMP_Dropdown CompoundFilterDropdown;
    public TextMeshProUGUI ResultText;

    private ShiritoriDictionary dict;

    private void Start()
    {
        dict = ShiritoriDictionary.Instance;
        if (dict == null)
        {
            ResultText.text = "Error: ShiritoriDictionary.Instance が見つかりません";
            return;
        }

        // Dropdown 初期化
        CompoundFilterDropdown.ClearOptions();
        CompoundFilterDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "複合語を禁止",
            "複合語を制限",
            "複合度を許可"
        });

        AllowProperNounDropdown.ClearOptions();
        AllowProperNounDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "固有名詞（人名を除く）を禁止",
            "固有名詞（人名を除く）を許可",
        });

        CheckButton.onClick.AddListener(OnCheckButtonClicked);
        ResultText.text = "読みを入力してチェックしてください。";
    }

    private void OnCheckButtonClicked()
    {
        string reading = ReadingInput.text.Trim();

        if (string.IsNullOrEmpty(reading))
        {
            ResultText.text = "入力が空です。";
            return;
        }

        // compound フィルタを選択
        CompoundFilterMode compoundFilter = GetSelectedCompoundFilter();

        // 固有名詞許可
        bool allowProperNoun = AllowProperNounDropdown.value != 0;

        // 存在チェック
        bool exists = dict.HasReadingWithCondition(
            reading,
            allowProperNoun,
            compoundFilter
        );

        if (!exists)
        {
            ResultText.text = $"「{reading}」は辞書にありません（条件に合致しません）。";
            return;
        }

        // surface を取得
        if (dict.TryGetSurfaceWithCondition(
                reading,
                out string surface,
                allowProperNoun,
                compoundFilter))
        {
            ResultText.text = $"存在します！\n表記: {surface}";
        }
        else
        {
            ResultText.text = $"「{reading}」は存在しますが surface の取得に失敗しました。";
        }
    }

    private CompoundFilterMode GetSelectedCompoundFilter()
    {
        switch (CompoundFilterDropdown.value)
        {
            case 0: return CompoundFilterMode.Strict;
            case 1: return CompoundFilterMode.Limited;
            default: return CompoundFilterMode.All;
        }
    }
}
