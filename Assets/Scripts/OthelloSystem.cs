using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OthelloSystem: MonoBehaviour
{
    const int FIELD_SIZE_X = 8;
    const int FIELD_SIZE_Y = 8;

    // ブロックの状態
    public enum ePieceState{
        None,
        Face,   //表か
        Back,   //裏か
        Max
    };

    // ボードの実体
    private GameObject _boardObject = null;
    private GameObject _cursorObject = null;
    // ブロックの実体
    private GameObject[,] _fieldPieceObject = new GameObject[FIELD_SIZE_Y, FIELD_SIZE_X];
    private OthelloPiece[,] _fieldPieces = new OthelloPiece[FIELD_SIZE_Y, FIELD_SIZE_X];
    // 最終的なブロックの状態
    private ePieceState[,] _fieldPieceStateFinal = new ePieceState[FIELD_SIZE_Y, FIELD_SIZE_X];
    [SerializeField] GameObject _boardPrefab = null;
    [SerializeField] GameObject _PiecePrefab = null;
    [SerializeField] GameObject _cursorPrefab = null;

    // カーソル制御用
    private int _cursorX = 0;
    private int _cursorY = 0;

    // ターン制御
    private ePieceState _turn = ePieceState.Face;

    // ひっくり返す対象
    class Position{
        public int _x;
        public int _y;

        public Position(int x, int y){
            _x = x;
            _y = y;
        }
    }

    // ひっくり返し処理の方向
    int[] TURN_CHECK_X = new int[] {-1, -1, 0, 1, 1, 1, 0, -1};
    int[] TURN_CHECK_Y = new int[] {0, 1, 1, 1, 0, -1, -1, -1};

    // Start is called before the first frame update
    void Start()
    {
        // 初期状態の設定
        for (int i = 0; i < FIELD_SIZE_Y; i++){
            for (int j = 0; j < FIELD_SIZE_X; j++){
                // ブロックの実体
                GameObject newObject = GameObject.Instantiate<GameObject>(_PiecePrefab);
                OthelloPiece newPiece = newObject.GetComponent<OthelloPiece>();
                newObject.transform.localPosition = new Vector3(-(FIELD_SIZE_X - 1.0f) * 0.5f + j, 0.125f, -(FIELD_SIZE_Y - 1.0f) * 0.5f + i);
                _fieldPieceObject[i, j] = newObject;
                _fieldPieces[i, j] = newPiece;
                // ブロックの状態
                _fieldPieceStateFinal[i, j] = ePieceState.None;
            }
            _fieldPieceStateFinal[3, 3] = ePieceState.Face;
            _fieldPieceStateFinal[4, 3] = ePieceState.Back;
            _fieldPieceStateFinal[3, 4] = ePieceState.Back;
            _fieldPieceStateFinal[4, 4] = ePieceState.Face;

            // ボードの生成
            _boardObject = GameObject.Instantiate<GameObject>(_boardPrefab);

            // カーソルの生成
            _cursorObject = GameObject.Instantiate<GameObject>(_cursorPrefab);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // カーソルの移動
        int deltaX = 0;
        int deltaY = 0;
        if (GetKeyEx(KeyCode.UpArrow)){
            deltaY += 1;
        }
        if (GetKeyEx(KeyCode.DownArrow)){
            deltaY -= 1;
        }
        if (GetKeyEx(KeyCode.LeftArrow)){
            deltaX -= 1;
        }
        if (GetKeyEx(KeyCode.RightArrow)){
            deltaX += 1;
        }
        _cursorX += deltaX;
        _cursorY += deltaY;
        _cursorObject.transform.localPosition = new Vector3(-(FIELD_SIZE_X - 1.0f) * 0.5f + _cursorX, 0.0f, -(FIELD_SIZE_Y - 1.0f) * 0.5f + _cursorY);

        if (GetKeyEx(KeyCode.Return)){
            if(0 <= _cursorX && _cursorX < FIELD_SIZE_X && 0 <= _cursorY && _cursorY < FIELD_SIZE_Y
                && _fieldPieceStateFinal[_cursorY, _cursorX] == ePieceState.None && Turn(false) > 0){
                _fieldPieceStateFinal[_cursorY, _cursorX] = _turn;
                Turn(true);
                _turn = ((_turn == ePieceState.Back) ? ePieceState.Face : ePieceState.Back);
            }
        }

        // ブロックの状態を更新
        UpdatePieceState();
    }

    int Turn(bool isTurn){

        // 敵の色
        ePieceState enemyColor = ((_turn == ePieceState.Back) ? ePieceState.Face : ePieceState.Back);

        // ひっくり返せる数
        bool isValidTurn = false;   //  ひっくり返せるかどうか
        List<Position> positionList = new List<Position>();
        int count = 0;

        // 左
        int deltaX = 0, deltaY = 0;
        for(int i = 0; i < TURN_CHECK_X.Length; i++){

            int x = _cursorX;
            int y = _cursorY;
            deltaX = TURN_CHECK_X[i];
            deltaY = TURN_CHECK_Y[i];
            isValidTurn = false;
            positionList.Clear();

            while(true){

                x += deltaX;
                y += deltaY;

                if(!(0 <= x && x < FIELD_SIZE_X && 0 <= y && y < FIELD_SIZE_Y)){
                    // 範囲外
                    break;
                }
                if(_fieldPieceStateFinal[y, x] == enemyColor){
                    // ひっくり返す対象
                    positionList.Add(new Position(x, y));
                }
                else if(_fieldPieceStateFinal[y, x] == _turn){
                    // ひっくり返せる
                    isValidTurn = true;
                    break;
                }
                else{
                 // 何もなし
                    break;
                }
            }

            //  実際のひっくり返し処理
            if(isValidTurn){
                count += positionList.Count;
                if(isTurn){
                    for(int j = 0; j < positionList.Count; j++){
                        Position pos = positionList[j];
                        _fieldPieceStateFinal[pos._y, pos._x] = _turn;
                    }
                }
            }
        }

        return count;
    }

    void UpdatePieceState(){
        // ブロックの状態反映
        for(int i = 0; i < FIELD_SIZE_Y; i++){
            for(int j = 0; j < FIELD_SIZE_X; j++){
                // ブロックの状態
                _fieldPieces[i, j].SetState(_fieldPieceStateFinal[i, j]);
            }
        }
    }

    // キー入力
    private Dictionary<KeyCode, int> _keyImputTimer = new Dictionary<KeyCode, int>();

    private bool GetKeyEx(KeyCode keyCode){
        if(!_keyImputTimer.ContainsKey(keyCode)){
            _keyImputTimer.Add(keyCode, -1);
        }

        if(Input.GetKey(keyCode)){
            _keyImputTimer[keyCode]++;
        }
        else{
            _keyImputTimer[keyCode] = -1;
        }
        return (_keyImputTimer[keyCode] == 0 || _keyImputTimer[keyCode] >= 10);
    }
}
