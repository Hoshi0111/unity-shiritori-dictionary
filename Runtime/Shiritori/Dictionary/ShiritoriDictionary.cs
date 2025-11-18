using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using System.Linq;
using static UnityEngine.EventSystems.EventTrigger;
using System.Diagnostics.Tracing;

namespace Shiritori.Dictionary
{
    public class ShiritoriDictionary : MonoBehaviour
    {
        // Resources 内の CSV のパス
        private const string CsvResourcePath = "Datasets/best_shiritori_words";

        // Resources 内の txt ファイルのパス
        private const string NgWordResourcePath = "UserData/ng_words";

        // Singleton 的にアクセスしたい場合用
        public static ShiritoriDictionary Instance { get; private set; }

        // key: reading（ひらがな）
        private readonly Dictionary<string, ShiritoriEntry> dict = new Dictionary<string, ShiritoriEntry>();

        // 最初の文字をkeyとした索引
        private Dictionary<char, List<ShiritoriEntry>> indexByHead = new Dictionary<char, List<ShiritoriEntry>>();

        private System.Random _random = new System.Random();

        private void Awake()
        {
            // Singleton セット
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            LoadCsvFromResources();
            BuildHeadIndex();
        }

        /// 開始直後に実行
        /// Resources から CSV(TextAsset) を自動ロードして辞書を構築
        private void LoadCsvFromResources()
        {
            var csvFile = Resources.Load<TextAsset>(CsvResourcePath);
            if (csvFile == null)
            {
                Debug.LogError($"ShiritoriDictionary: Resources から CSV が見つかりませんでした: {CsvResourcePath}");
                return;
            }

            dict.Clear();

            var text = csvFile.text;
            var lines = text.Split('\n');

            bool isFirst = true;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                // 1行目はヘッダのためスルー
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                var cols = line.Split(',');
                if (cols.Length < 5)
                {
                    Debug.LogWarning("列数が足りない行をスキップ: " + line);
                    continue;
                }

                var reading = cols[0].Trim();
                var surface = cols[1].Trim();
                var pos2 = cols[2].Trim();
                var pos3 = cols[3].Trim();
                var compound = cols[4].Trim();

                if (string.IsNullOrEmpty(reading))
                    continue;

                var entry = new ShiritoriEntry
                {
                    Reading = reading,
                    Surface = surface,
                    Pos2 = pos2,
                    Pos3 = pos3,
                    Compound = compound
                };

                dict[reading] = entry;
            }

            Debug.Log($"ShiritoriDictionary: CSV 読み込み完了。エントリ数 = {dict.Count}");
        }

        private void BuildHeadIndex()
        {
            indexByHead.Clear();

            foreach (var kv in dict)
            {
                var entry = kv.Value;
                if (string.IsNullOrEmpty(entry.Reading))
                    continue;

                char head = entry.Reading[0];

                if (!indexByHead.TryGetValue(head, out var list))
                {
                    list = new List<ShiritoriEntry>();
                    indexByHead[head] = list;
                }

                list.Add(entry);
            }
            Debug.Log($"索引読み込み完了。");

            // NGワードを除外する
            var ngFile = Resources.Load<TextAsset>(NgWordResourcePath);

            if(ngFile == null)
            {
                Debug.LogWarning($"ShiritoriDictionary: NGワードファイルは見つかりませんでした");
                return;
            }

            var ngWords = ngFile.text.Split("\r");

            // NGワードセットを構築
            var ngSet = new HashSet<string>();

            foreach (var word in ngWords)
            {
                var trimmed = word.Trim();
                if(!string.IsNullOrEmpty(trimmed))
                {
                    ngSet.Add(trimmed);
                }
            }

            // NGワードを索引から除外
            foreach (var kv in indexByHead)
            {
                var list = kv.Value;
                list.RemoveAll(entry => ngSet.Contains(entry.Reading));
            }

            Debug.Log($"索引データベースからNGワード除外完了。NGワード数:{ngSet.Count}");

            // もし検索データセットからも消したい場合以下を有効化
            //foreach (var ngWord in ngSet)
            //{
            //    dict.Remove(ngWord);
            //}
            //Debug.Log($"検索データベースからNGワード除外完了");

        }

        // ======================
        //    公開 API 群
        // ======================

        /// <summary>
        /// 指定した読みが辞書に存在するかどうか
        /// </summary>
        public bool HasReading(string reading)
        {
            if (string.IsNullOrEmpty(reading)) return false;
            return dict.ContainsKey(reading);
        }

        /// <summary>
        /// 読みが一致するエントリの surface をそのまま返す
        /// </summary>
        public bool TryGetSurface(string reading, out string surface)
        {
            surface = null;
            if (string.IsNullOrEmpty(reading)) return false;

            if (dict.TryGetValue(reading, out var entry))
            {
                surface = entry.Surface;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 読みが一致するエントリの surfaceを返り値とする、ない場合はNullで返す
        /// </summary>
        public string GetSurfaceOrNull(string reading)
        {
            if (dict.TryGetValue(reading, out var entry))
            {
                return entry.Surface;   // 見つかった場合
            }
            return null;               // 見つからなかった場合
        }

        /// <summary>
        /// 条件付きで「読みが一致する有効な単語があるか」を確認
        /// allowProperNoun: false → 固有名詞を除外
        /// compoundFilter : All / Limited / Strict
        /// </summary>
        public bool HasReadingWithCondition(
            string reading,
            bool allowProperNoun = true,
            CompoundFilterMode compoundFilter = CompoundFilterMode.All)
        {
            if (string.IsNullOrEmpty(reading)) return false;

            if (!dict.TryGetValue(reading, out var entry))
                return false;

            if (!allowProperNoun && entry.Pos2 == "固有名詞")
                return false;

            if (!IsCompoundAllowed(entry.Compound, compoundFilter))
                return false;

            return true;
        }

        /// <summary>
        /// 条件付きで surface を取得する
        /// </summary>
        public bool TryGetSurfaceWithCondition(
            string reading,
            out string surface,
            bool allowProperNoun = true,
            CompoundFilterMode compoundFilter = CompoundFilterMode.All)
        {
            surface = null;
            if (string.IsNullOrEmpty(reading)) return false;

            if (!dict.TryGetValue(reading, out var entry))
                return false;

            if (!allowProperNoun && entry.Pos2 == "固有名詞")
                return false;

            if (!IsCompoundAllowed(entry.Compound, compoundFilter))
                return false;

            surface = entry.Surface;
            return true;
        }

        /// <summary>
        /// あるひらがなから始まる単語をランダムに一つ選択して返す
        /// </summary>
        public bool TryGetRandomByInitial(char head, out string reading, out string surface)
        {
            reading = null;
            surface = null;

            if (!indexByHead.TryGetValue(head, out var list))
                return false;

            if (list.Count == 0)
                return false;

            var choice = list[_random.Next(list.Count)];

            reading = choice.Reading;
            surface = choice.Surface;
            return true;
        }

        /// <summary>
        /// あるひらがなから始まる単語をリスト化して返す
        /// </summary>
        public bool TryGetListByInitial(char head, out List<ShiritoriEntry> entryList)
        {
            entryList = null;

            if (!indexByHead.TryGetValue(head, out var list))
                return false;

            if (list.Count == 0)
                return false;

            entryList = list;
            return true;
        }

        /// <summary>
        /// 条件をもとにあるひらがなから始まる単語をランダムに一つ選択して返す
        /// </summary>
        public bool TryGetRandomByInitialWithCondition(char head, out string reading, out string surface, bool allowProperNoun, CompoundFilterMode filterMode)
        {
            reading = null;
            surface = null;

            if (!indexByHead.TryGetValue(head, out var allList))
                return false;

            var candidates = dict.Where(kvp =>
                    kvp.Key.Length > 0 &&
                    kvp.Key[0] == head &&
                    (kvp.Value.Pos2 == "普通名詞" || allowProperNoun) &&
                    !(kvp.Value.Compound == "C" && !(filterMode == CompoundFilterMode.All) || kvp.Value.Compound == "B" && filterMode == CompoundFilterMode.Strict))
                .Select(kvp => kvp.Value)
                .ToList();

            if (candidates.Count == 0)
                return false;

            var choice = candidates[_random.Next(candidates.Count)];

            reading = choice.Reading;
            surface = choice.Surface;

            return true;
        }


        /// <summary>
        /// 条件をもとにあるひらがなから始まる単語をランダムに一つ選択して返す
        /// </summary>
        public bool TryGetListByInitialWithCondition(char head, out List<ShiritoriEntry> entryList, bool allowProperNoun, CompoundFilterMode filterMode)
        {
            entryList = null;

            if (!indexByHead.TryGetValue(head, out var allList))
                return false;

            var candidates = dict.Where(kvp =>
                    kvp.Key.Length > 0 &&
                    kvp.Key[0] == head &&
                    (kvp.Value.Pos2 == "普通名詞" || allowProperNoun) &&
                    !(kvp.Value.Compound == "C" && !(filterMode == CompoundFilterMode.All) || kvp.Value.Compound == "B" && filterMode == CompoundFilterMode.Strict))
                .Select(kvp => kvp.Value)
                .ToList();

            if (candidates.Count == 0)
                return false;

            entryList = candidates;

            return true;
        }

        // ======================
        //    優先度/条件ヘルパ
        // ======================

        private bool IsCompoundAllowed(string compound, CompoundFilterMode mode)
        {
            switch (mode)
            {
                case CompoundFilterMode.Strict:
                    return compound == "A";

                case CompoundFilterMode.Limited:
                    return compound == "A" || compound == "B";

                case CompoundFilterMode.All:
                    return compound == "A" || compound == "B" || compound == "C";

                default:
                    return true;
            }
        }
    }
}
