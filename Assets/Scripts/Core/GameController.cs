using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Startup")]
    public int startLevelId = 1;

    [Header("References")]
    public LevelLoader levelLoader;
    public InputController inputController;
    public ArrowFactory arrowFactory;
    public Transform boardRoot;

    private GameState currentState = GameState.None;
    private BoardModel boardModel;
    private LevelData currentLevelData;
    private readonly Dictionary<int, ArrowRoot> arrowMap = new Dictionary<int, ArrowRoot>();
    private int remainingArrowCount;

    private void Start()
    {
        if (levelLoader == null)
        {
            levelLoader = GetComponent<LevelLoader>();
        }

        if (inputController == null)
        {
            inputController = FindObjectOfType<InputController>();
        }

        if (arrowFactory == null)
        {
            arrowFactory = FindObjectOfType<ArrowFactory>();
        }

        if (boardRoot == null && arrowFactory != null)
        {
            boardRoot = arrowFactory.boardRoot;
        }

        if (inputController != null)
        {
            inputController.Initialize(this);
        }

        StartGame(startLevelId);
    }

    public void StartGame(int levelId)
    {
        SetGameState(GameState.Loading);
        ClearCurrentLevel();

        if (levelLoader == null)
        {
            Debug.LogError("GameController missing LevelLoader.");
            return;
        }

        if (arrowFactory == null)
        {
            Debug.LogError("GameController missing ArrowFactory.");
            return;
        }

        currentLevelData = levelLoader.LoadLevel(levelId);

        if (!levelLoader.ValidateLevel(currentLevelData))
        {
            SetGameState(GameState.Fail);
            return;
        }

        boardModel = new BoardModel();
        boardModel.Initialize(currentLevelData);
        boardModel.RegisterAllArrows(currentLevelData.arrows);

        foreach (ArrowData arrowData in currentLevelData.arrows)
        {
            ArrowRoot arrowRoot = arrowFactory.CreateArrow(arrowData, currentLevelData);

            if (arrowRoot != null)
            {
                arrowMap[arrowData.id] = arrowRoot;
            }
        }

        remainingArrowCount = arrowMap.Count;

        SetGameState(GameState.Playing);

        if (inputController != null)
        {
            inputController.SetInputEnabled(true);
        }

        Debug.Log($"Level {levelId} started. Arrows: {remainingArrowCount}");
    }

    public void ClearCurrentLevel()
    {
        if (inputController != null)
        {
            inputController.SetInputEnabled(false);
        }

        arrowMap.Clear();
        remainingArrowCount = 0;

        if (boardRoot != null)
        {
            for (int i = boardRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(boardRoot.GetChild(i).gameObject);
            }
        }

        if (boardModel != null)
        {
            boardModel.Clear();
        }
    }

    public void OnArrowClicked(ArrowRoot arrowRoot)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        if (arrowRoot == null)
        {
            return;
        }

        if (arrowRoot.State != ArrowState.Idle)
        {
            return;
        }

        if (boardModel == null)
        {
            return;
        }

        bool canFlyOut = boardModel.CanFlyOut(arrowRoot.Data);

        if (canFlyOut)
        {
            Debug.Log($"箭头 {arrowRoot.Data.id} 能发射");
            HandleArrowCanFly(arrowRoot);
        }
        else
        {
            Debug.Log($"箭头 {arrowRoot.Data.id} 被阻挡");
            HandleArrowBlocked(arrowRoot);
        }
    }

    public void HandleArrowCanFly(ArrowRoot arrowRoot)
    {
        boardModel.RemoveArrow(arrowRoot.Data);
        arrowRoot.DisableHitArea();

        arrowRoot.PlayFlyOutPlaceholder(() =>
        {
            OnArrowFlyOutFinished(arrowRoot);
        });
    }

    public void HandleArrowBlocked(ArrowRoot arrowRoot)
    {
        arrowRoot.PlayBlockedFeedbackPlaceholder();
    }

    public void OnArrowFlyOutFinished(ArrowRoot arrowRoot)
    {
        if (arrowRoot == null)
        {
            return;
        }

        arrowRoot.SetState(ArrowState.Removed);

        if (arrowRoot.Data != null)
        {
            arrowMap.Remove(arrowRoot.Data.id);
        }

        remainingArrowCount--;
        CheckWin();
    }

    public void CheckWin()
    {
        if (remainingArrowCount <= 0)
        {
            SetGameState(GameState.Win);

            if (inputController != null)
            {
                inputController.SetInputEnabled(false);
            }

            Debug.Log("Level Complete");
        }
    }

    public void SetGameState(GameState state)
    {
        currentState = state;
        Debug.Log($"GameState: {currentState}");
    }

    public void RestartLevel()
    {
        int levelId = currentLevelData != null ? currentLevelData.levelId : startLevelId;
        StartGame(levelId);
    }
}