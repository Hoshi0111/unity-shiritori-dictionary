# ShiritoriDictionary クラス ドキュメント

日本語しりとり用の単語辞書を Unity 上で扱うためのクラスです。  
Resourcesに配置した CSV から辞書を読み込み、以下の機能を提供します：

- 読み（ひらがな）から単語が存在するか判定
- surface（表記）取得
- 固有名詞・複合度レベルによるフィルタ付き判定
- 指定したひらがなで始まる単語のランダム取得
- 指定したひらがなで始まる単語リストの取得

## 概要

*MonoBehaviour を継承したコンポーネント
*シングルトン (ShiritoriDictionary.Instance) としてどこからでも参照可能
*Awake() 時に CSV を自動ロード＆インデックス構築

*CSVの各行からShiritoriEntryを生成しdict[reading]に登録
 *cols[0]: reading（ひらがな）
 *cols[1]: surface（表記、複数表記は / で連結済み）
 *cols[2]: pos2（普通名詞 / 固有名詞など）
 *cols[3]: pos3（一般 / 地名 / 人名など）
 *cols[4]: compound（複合度 A/B/C）

## 公開API一覧

###HasReading

*目的：
指定した読み（ひらがな）の単語が辞書に存在するかどうかを判定する。

*引数：

reading - ひらがなによる読み（例：「がっこう」「ねこ」など）(string)

*戻り値：

true：その読みの単語が辞書に登録されている

false：登録されていない、または引数が空・null

*想定用途：

プレイヤーの入力した単語が「辞書に存在するか」をまず判定したいとき。

###TryGetSurface

*目的：
指定した読みに対応する表記（surface）を取得する。

*引数：

reading - ひらがな読み(string)

surface（out）- 対応する表記（漢字・ひらがななど）。複数候補がある場合は / 区切りで連結されている想定（例：「紙/神」）(string)

*戻り値：

true：該当する単語が見つかり、surface に値がセットされた

false：見つからず、surface は null のまま

*想定用途：

入力チェック後に、実際に画面に出す表記を知りたいとき。

ログ出力やデバッグ表示など。

###GetSurfaceOrNull

*目的：
読みに対応する表記を、戻り値としてシンプルに取得する。

*引数：

reading(string) - ひらがな読み

*戻り値：

対応する表記（surface)。存在しない場合は null。(string)

*想定用途：

「存在すればその表記、なければ null」というスタイルで書きたいとき。

if (GetSurfaceOrNull(reading) != null) のように簡潔に判定したい場合。

###HasReadingWithCondition

*目的：
読みが辞書に存在するだけでなく、「固有名詞を含めるか」「複合度レベルをどこまで許可するか」といった条件付きで有効な単語かどうかを判定する。

*引数：

reading - ひらがな読み(string)

allowProperNoun（デフォルト：true）- false の場合、「固有名詞」を除外する/true の場合、固有名詞も許可(bool)

compoundFilter（デフォルト：CompoundFilterMode.All）- 複合度レベル（A/B/C）をどこまで許可するか(CompoundFilterMode)

 *Strict：A のみ
 *Limited：A/B まで
 *All：A/B/C すべて

*戻り値：

true：条件を満たす単語が存在する

false：辞書に存在しない、または条件を満たさない

*想定用途：

「しりとりで地名や人名を禁止したい」「複合語っぽい単語を弱くしたい」といったゲームルールを実装するとき。

###TryGetSurfaceWithCondition

*目的：
条件付き（固有名詞・複合度）で、対応する表記を取得する。

*引数：

reading - ひらがな読み(string)

surface（out）- 条件を満たす場合、その表記がセットされる(string)

allowProperNoun - 固有名詞を許可するかどうか（HasReadingWithCondition と同じ）(bool)

compoundFilter - 許可する複合度レベル(CompoundFilterMode)

*戻り値：

true：条件を満たす単語が存在し、surface に値がセットされる

false：存在しない、または条件を満たさない（surface は null）

*想定用途：

単に存在判定だけでなく、「条件を満たす表記をそのまま使いたい」場合。

対戦相手に提示する単語表示など。

###TryGetRandomByInitial

*目的：
指定した 1 文字のひらがなから始まる単語を、ランダムに 1 つ取得する。

*引数：

head - 先頭のひらがな 1 文字(char)

reading（out）- ランダムに選ばれた単語の読み(string)

surface（out）- ランダムに選ばれた単語の表記(string)

*戻り値：

true：その文字から始まる単語が少なくとも 1 つ存在し、ランダムに取得できた

false：該当する単語が 1 つも存在しない

*想定用途：

CPU プレイヤーの手番で、「前の単語の末尾の文字から始まる単語」を自動的に選ぶとき。

単語候補をランダムに提示する機能。

###TryGetListByInitial

*目的：
指定したひらがなから始まる単語のリストを取得する。

*引数：

head - 先頭のひらがな 1 文字(char)

entryList（out）- 見つかった単語の ShiritoriEntry リスト(List<ShiritoriEntry>)

*戻り値：

true：その文字から始まる単語が 1 件以上存在し、リストが取得できた

false：該当する単語が存在しない（entryList は null）

*想定用途：

デバッグや検証用途で、「ある文字から始まる単語を一覧で確認したい」場合。

「文字数は8文字以下」「指定のひらがなで終わる」など固有の制限を設けて選択する場合。

###TryGetRandomByInitialWithCondition

*目的：
指定したひらがなから始まる単語のうち、「固有名詞を含めるか」「複合度レベルフィルタ」を考慮しつつ、条件を満たすものからランダムに 1 つ選ぶ。

*引数：

head - 先頭のひらがな 1 文字(char)

reading（out）- ランダムに選ばれた単語の読み(string)

surface（out）- ランダムに選ばれた単語の表記(string)

allowProperNoun - 固有名詞を許可するかどうか(bool)

compoundFilter - 許可する複合度レベル(CompoundFilterMode)

*戻り値：

true：条件を満たす候補が存在し、その中から 1 つを選べた

false：条件を満たす候補が 1 つも存在しない

*想定用途：

CPU が「固有名詞を含めるか」「複合度レベルフィルタ」といった制限付きで単語を選ぶ AI を作るとき。

###TryGetListByInitialWithCondition

*目的：
指定したひらがなから始まる単語のうち、条件（固有名詞・複合度）を満たすものをリストとして取得する。

*引数：

head - 先頭のひらがな 1 文字(char)

entryList（out）- 見つかった単語の ShiritoriEntry リスト(List<ShiritoriEntry>)

allowProperNoun - 固有名詞を許可するかどうか(bool)

compoundFilter - 許可する複合度レベル(CompoundFilterMode)

*戻り値：

true：条件を満たす単語が 1 件以上存在し、リストが取得できた

false：条件を満たす単語が存在しない（entryList は null）

*想定用途：

デバッグ用に「条件込みの候補一覧」を見たいとき。

UI 上で、ユーザーに選択させる候補リストを出すとき。

「固有名詞を含めるか」「複合度レベルフィルタ」を考慮しつつ、「文字数は8文字以下」「指定のひらがなで終わる」など固有の制限を設けて選択する場合。