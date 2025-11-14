using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shiritori.Dictionary;

public class DemoShiritoriSceneController : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField ReadingInput;
    public TMP_InputField InitialInput;
    public Button CheckButton;
    public Button GenerateButton;
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
        GenerateButton.onClick.AddListener(OnGenerateButtonClicked);
        ResultText.text = "入力して検索か生成してください";
    }

    // 入力された単語があるかの判定
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

    //　入力された文字から始まる単語を生成
    private void OnGenerateButtonClicked()
    {
        string initial = InitialInput.text.Trim();

        if (string.IsNullOrEmpty(initial))
        {
            ResultText.text = "入力が空です。";
            return;
        }

        if(initial.Length != 1)
        {
            ResultText.text = "入力が不正です。";
            return;
        }

        char init = initial[0];
        if (init < 'ぁ' || init > 'ゖ')
        {
            ResultText.text = "入力が不正です。";
            return;
        }

        // compound フィルタを選択
        CompoundFilterMode compoundFilter = GetSelectedCompoundFilter();

        // 固有名詞許可
        bool allowProperNoun = AllowProperNounDropdown.value != 0;

        if (dict.TryGetRandomByInitialWithCondition(
                init, 
                out string nextReading, 
                out string nextSurface,
                allowProperNoun,
                compoundFilter))
        {
            ResultText.text = $"次の単語：{nextSurface} ({nextReading})";
        }
        else
        {
            ResultText.text = $"{initial}に続く単語がありません";
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
