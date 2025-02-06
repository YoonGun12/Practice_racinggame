using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    //Enemey
    public GameObject[] enemyPrefabs;
    public int poolSize = 3;
    private Queue<GameObject> enemyPool;

    public Transform spanwPoint;
    public float firstSpawnTime = 5f;
    public float secondSpawnTime = 3f;
    public float thirdSpawnTime = 1f;

    private float timer;
    private bool isGameOver = false;
    
    // 프리팹
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject roadPrefab;

    // UI 관련
    [SerializeField] private MoveButton leftMoveButton;
    [SerializeField] private MoveButton rightMoveButton;
    [SerializeField] private TMP_Text gasText;
    [SerializeField] private GameObject startPanelPrefab;
    [SerializeField] private GameObject endPanelPrefab;
    [SerializeField] private Transform canvasTransform;
    
    // 자동차
    private CarController _carController;
    
    // 도로 오브젝트 풀
    private Queue<GameObject> _roadPool = new Queue<GameObject>();
    private int _roadPoolSize = 3;
    
    // 도로 이동
    private List<GameObject> _activeRoads = new List<GameObject>();
    
    // 만들어지는 도로의 index
    private int _roadIndex;
    
    // 상태
    public enum State { Start, Play, End }
    public State GameState { get; private set; }
    
    // 싱글턴
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        
    }

    private void Start()
    {
        // Road 오브젝트 풀 초기화
        InitializeRoadPool();

        InitializeEnemyPool();
        StartCoroutine(SpawnEnemies());
        
        // 게임 상태 Start로 변경
        GameState = State.Start;
        
        // Start Panel 표시
        ShowStartPanel();
    }

    private void Update()
    {
        // 게임 상태에 따라 동작
        switch (GameState)
        {
            case State.Start:
                break;
            case State.Play:
                // 활성화 된 도로를 아래로 서서히 이동
                foreach (var activeRoad in _activeRoads)
                {
                    activeRoad.transform.Translate(-Vector3.forward * Time.deltaTime);
                }
        
                // Gas 정보 출력
                if (_carController != null) gasText.text = _carController.Gas.ToString();
                break;
            case State.End:
                break;
        }

        timer += Time.deltaTime;
    }

    private void StartGame()
    {
        // _roadIndex 초기화
        _roadIndex = 0;
        
        // 도로 생성
        SpawnRoad(Vector3.zero);
        
        // 자동차 생성
        _carController = Instantiate(carPrefab, new Vector3(0, 0, -3f), Quaternion.identity)
            .GetComponent<CarController>();
        
        // Left, Right move button에 자동차 컨트롤 기능 적용
        leftMoveButton.OnMoveButtonDown += () => _carController.Move(-1f);
        rightMoveButton.OnMoveButtonDown += () => _carController.Move(1f);
        
        // 게임 상태를 Play로 변경
        GameState = State.Play;
    }

    public void EndGame()
    {
        // 게임 상태 변경
        GameState = State.End;
        
        // 자동차 제거
        Destroy(_carController.gameObject);
        
        // 도로 제거
        foreach (var activeRoad in _activeRoads)
        {
            activeRoad.SetActive(false);
        }
        
        // 게임 오버 패널 표시
        ShowEndPanel();
    }

    #region UI

    /// <summary>
    /// 시작 화면 표시
    /// </summary>
    private void ShowStartPanel()
    {
        StartPanelController startPanelController = Instantiate(startPanelPrefab, canvasTransform)
            .GetComponent<StartPanelController>();
        startPanelController.OnStartButtonClick += () =>
        {
            StartGame();
            Destroy(startPanelController.gameObject);
        };
    }

    /// <summary>
    /// 게임오버 화면 표시
    /// </summary>
    private void ShowEndPanel()
    {
        StartPanelController endPanelController = Instantiate(endPanelPrefab, canvasTransform)
            .GetComponent<StartPanelController>();
        endPanelController.OnStartButtonClick += () =>
        {
            Destroy(endPanelController.gameObject);
            ShowStartPanel();
        };
    }

    #endregion
    
    #region 도로 생성 및 관리

    /// <summary>
    /// 도로 오브젝트 풀 초기화
    /// </summary>
    private void InitializeRoadPool()
    {
        for (int i = 0; i < _roadPoolSize; i++)
        {
            GameObject road = Instantiate(roadPrefab);
            road.SetActive(false);
            _roadPool.Enqueue(road);
        }    
    }
    
    /// <summary>
    /// 도로 오브젝트 풀에서 불러와 배치하는 함수
    /// </summary>
    public void SpawnRoad(Vector3 position)
    {
        GameObject road;
        if (_roadPool.Count > 0)
        {
            road = _roadPool.Dequeue();
            road.transform.position = position;
            road.SetActive(true);
        }
        else
        {
            road = Instantiate(roadPrefab, position, Quaternion.identity);
        }
        
        // 가스 아이템 생성
        if (_roadIndex > 0 && _roadIndex % 2 == 0)
        {
            Debug.Log("Spawn Gas Road Index: " + _roadIndex);
            road.GetComponent<RoadController>().SpawnGas();
        }
        
        // 활성화 된 길을 움직이기 위해 List에 저장
        _activeRoads.Add(road);
        _roadIndex++;
    }

    public void DestroyRoad(GameObject road)
    {
        road.SetActive(false);
        _activeRoads.Remove(road);
        _roadPool.Enqueue(road);
    }
    
    #endregion


    void InitializeEnemyPool()
    {
        enemyPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            foreach (var enemy in enemyPrefabs)
            {
                GameObject obj = Instantiate(enemy);
                obj.SetActive(false);
                enemyPool.Enqueue(obj);
            }
        }
    }

    GameObject GetEnemyFromPool()
    {
        if (enemyPool.Count > 0)
        {
            GameObject obj = enemyPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)]);
            return obj;
        }
    }

    public void ReturnEnemyToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        enemyPool.Enqueue(enemy);
    }

    IEnumerator SpawnEnemies()
    {
        while (!isGameOver)
        {
            float currentPhase = GetCurrentPhase();

            GameObject enemy = GetEnemyFromPool();
            enemy.transform.position = spanwPoint.position;
            enemy.GetComponent<EnemyController>().ActivateEnemy(timer);

            yield return new WaitForSeconds(currentPhase);
        }
    }

    float GetCurrentPhase()
    {
        if (timer < 30f) return firstSpawnTime;
        if (timer < 60f) return secondSpawnTime;
        return thirdSpawnTime;
    }

    public void GameOver()
    {
        isGameOver = true;
        
    }

}
