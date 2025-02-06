using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GameTestScript
{
    private CarController _carController;
    private GameObject _leftMoveButton;
    private GameObject _rightMoveButton;
    
    // A Test behaves as an ordinary method
    [Test]
    public void GameTestScriptSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator GameTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.

        //타임 스케일 변경
        Time.timeScale = 20f;
        
        //씬 로드하기
        SceneManager.LoadScene("Scenes/Game", LoadSceneMode.Single);
        yield return waitForSceneLoad();

        //필수 오브젝트 확인
        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Assert.IsNotNull(gameManager, "GameManager is null");

        var startButton = GameObject.Find("Start Button");
        Assert.IsNotNull(startButton, "Start Button is Null");
        
        
        //게임 실행
        startButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        
        //플레이어 자동차 확인
        _carController = GameObject.Find("Car(Clone)").GetComponent<CarController>();
        Assert.IsNotNull(_carController, "CarController is Null");
        
        //게임 제어 관련 버튼 확인
        _leftMoveButton = GameObject.Find("LeftMoveButton");
        Assert.IsNotNull(_leftMoveButton, "Left Move Button is Null");
        _rightMoveButton = GameObject.Find("RightMoveButton");
        Assert.IsNotNull(_rightMoveButton, "Right Move Button is Null");

        //가스의 등장 위치 파악하기
        Vector3 leftPosition = new Vector3(-1f, 0.2f, -3f);
        Vector3 rightPosition = new Vector3(1f, 0.2f, -3f);
        Vector3 centerPosition = new Vector3(0f, 0.2f, -3f);

        float rayDistance = 7f;
        Vector3 rayDirection = Vector3.forward;
        
        //플레이 시간
        float elapsedTime = 0f;
        float targetTime = 10f;
        
        //반복
        while (gameManager.GameState == GameManager.State.Play)
        {
            RaycastHit hit;
            if (Physics.Raycast(leftPosition, rayDirection, out hit, rayDistance, LayerMask.GetMask("Gas")))
            {
                MoveCar(hit.point);
            }
            else if (Physics.Raycast(rightPosition, rayDirection, out hit, rayDistance, LayerMask.GetMask("Gas")))
            {
                MoveCar(hit.point);
            }
            else if (Physics.Raycast(centerPosition, rayDirection, out hit, rayDistance,LayerMask.GetMask("Gas")))
            {
                MoveCar(hit.point);
            }
            else
            {
                MoveButtonUp(_leftMoveButton);
                MoveButtonUp(_rightMoveButton);
            }
            
            Debug.DrawRay(leftPosition, rayDirection * rayDistance, Color.red);
            Debug.DrawRay(rightPosition, rayDirection * rayDistance, Color.green);
            Debug.DrawRay(centerPosition, rayDirection * rayDistance, Color.blue);

            elapsedTime += Time.deltaTime;
            
            yield return null;
        }

        if (elapsedTime < targetTime)
        {
            Assert.Fail("Game Time is too short");
        }

        Time.timeScale = 1f;

    }

    private IEnumerator waitForSceneLoad()
    {
        {
            while (SceneManager.GetActiveScene().buildIndex > 0)
            {
                yield return null;
            }
        }
    }

    //MoveButton 다운
    private void MoveButtonDown(GameObject moveButton)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(moveButton, pointerEventData, ExecuteEvents.pointerDownHandler);
    }
    
    //MoveButton 업
    private void MoveButtonUp(GameObject moveButton)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(moveButton, pointerEventData, ExecuteEvents.pointerUpHandler);
    }

    //플레이어 자동차 이동
    private void MoveCar(Vector3 targetPosition)
    {
        if (Mathf.Abs(targetPosition.x - _carController.transform.position.x) < 0.1f)
        {
            MoveButtonUp(_leftMoveButton);
            MoveButtonUp(_rightMoveButton);
            return;
        }
        
        if (targetPosition.x < _carController.transform.position.x)
        {
            //왼쪽으로 이동
            MoveButtonDown(_leftMoveButton);
            MoveButtonUp(_rightMoveButton);
        }
        else if (targetPosition.x > _carController.transform.position.x)
        {
            //오른쪽으로 이동
            MoveButtonDown(_rightMoveButton);
            MoveButtonUp(_leftMoveButton);
        }
        else
        {
            MoveButtonUp(_leftMoveButton);
            MoveButtonUp(_rightMoveButton);
        }
    }
}
