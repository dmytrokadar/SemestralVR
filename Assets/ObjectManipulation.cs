using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using PDollarGestureRecognizer;
using System.IO;
using UnityEngine.UI;
using System.Xml;

// inspiration: https://www.youtube.com/watch?v=GRSOrkmasMM&t=628s
public class ObjectManipulation : MonoBehaviour
{
    public XRNode inputR;
    public XRNode inputL;
    public InputHelpers.Button inputButton;
    public InputHelpers.Button inputButtonForTraining;
    public InputHelpers.Button inputButtonSave;
    public float inputThreshold = 0.2f;
    public float inputTrThreshold = 1f;
    public Transform movementSource;
    public Text displayTextTr;
    public Text displayGesture;
    public XRRayInteractor interactor;

    private bool isMoving = false;
    private List<Vector3> positionList = new List<Vector3>();

    //від цього залежить чи далеко будуть шаріки
    public float posThresholdDist = 0.03f;
    public float scoreThreshold = 0.8f;
    private float timeToWait = 2f;
    public GameObject drawingCubePrefab;
    private bool trainGesture = false;
    private string newGestureName = "Clothes_Rack";
    private List<Gesture> trainingSet = new List<Gesture>();
    private float spawnDistance = 0.5f;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // https://answers.unity.com/questions/777335/46-ui-changing-the-text-component-via-script.html

        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath, "*.xml");

        foreach(string fileN in gestureFiles)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(fileN));
        }
    }

    string CountModelSize()
    {
        return "big";
    }

    void SpawnObject(string obj)
    {
        //inspiration https://gamedevbeginner.com/how-to-spawn-an-object-in-unity-using-instantiate/
        //inspiration https://answers.unity.com/questions/772331/spawn-object-in-front-of-player-and-the-way-he-is.html
        Vector3 pos = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward;
        Quaternion rot = Camera.main.transform.rotation;
        Vector3 spawnPos = pos + direction*spawnDistance;

        GameObject newObject = (GameObject)Instantiate(Resources.Load(obj), spawnPos, rot);
        spawnedObjects.Add(newObject);
    }

    void StartMovement()
    {
        Debug.Log("Start movement");
        isMoving = true;
        positionList.Clear();
        positionList.Add(movementSource.position);

        if(drawingCubePrefab != null)
            //TODO поміняти на 3
            Destroy(Instantiate(drawingCubePrefab, movementSource.position, Quaternion.identity), 5);
    }

    void UpdateMovement()
    {
        Debug.Log("UPD movement");
        Vector3 lastPosition = positionList[positionList.Count - 1];
        if (Vector3.Distance(movementSource.position, lastPosition) > posThresholdDist)
        {
            positionList.Add(movementSource.position);
            if (drawingCubePrefab != null)
                //TODO поміняти на 3
                Destroy(Instantiate(drawingCubePrefab, movementSource.position, Quaternion.identity), 5);
        }
    }

    void EndMovement()
    {
        Debug.Log("End movement");
        isMoving = false;

        Point[] pointArray = new Point[positionList.Count];

        for(int i = 0; i < positionList.Count; i++)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(positionList[i]);
            pointArray[i] = new Point(screenPoint.x, screenPoint.y, 0);
        }

        Gesture newGesture = new Gesture(pointArray);

        if (trainGesture)
        {
            if (newGestureName != " ")
            {
                newGesture.Name = newGestureName;
                trainingSet.Add(newGesture);

                string fileForGesture = Application.persistentDataPath + "/" + newGestureName + ".xml";
                GestureIO.WriteGesture(pointArray, newGestureName, fileForGesture);
            }
        } else
        {
            Result result = PointCloudRecognizer.Classify(newGesture, trainingSet.ToArray());
            Debug.Log(result.GestureClass + " " + result.Score);
            if (result.Score >= scoreThreshold)
            {
                displayGesture.text = result.GestureClass + " " + result.Score;
                //TODO: cant bring out of if, recognized not displays
                StartCoroutine(enableText(displayGesture, timeToWait));
                SpawnObject(result.GestureClass);
            }
            else
            {
                //TODO спавнити напис з ерором
                displayGesture.text = "No results";
                StartCoroutine(enableText(displayGesture, timeToWait));
            }
        }
    }

    bool isPressedTr = false;

    IEnumerator enableText(Text text, float delay)
    {
        //inspiration https://answers.unity.com/questions/1809556/how-to-display-an-object-for-just-few-seconds-repe.html

        text.gameObject.SetActive(true);

        yield return new WaitForSeconds(delay);
        text.gameObject.SetActive(false);
    }

    void ToggleTrainingMode()
    {
        //InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputL), inputButtonForTraining, out bool tmp, inputTrThreshold);

        if (InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputL), inputButtonForTraining, out bool tmp, inputTrThreshold) && tmp && !isPressedTr)
        {
            trainGesture = !trainGesture;
            
            Debug.Log("Value Changed: " + trainGesture);
            isPressedTr = true;
            displayTextTr.text = "Training mode: " + trainGesture;

            StartCoroutine(enableText(displayTextTr, timeToWait));
        }
        if(!tmp)
            isPressedTr=false;
    }

    void SaveObjects()
    {
        if (InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputL), inputButtonSave, out bool isPressed, inputThreshold) && isPressed)
        {
            if(spawnedObjects.Count < 1)
            {
                Debug.Log("Nothing to save");
                displayTextTr.text = "Nothing to save";

                StartCoroutine(enableText(displayTextTr, timeToWait));
                return;
            }
            //inspiration https://www.youtube.com/watch?v=q878MDiaSVg&ab_channel=BeaverJoe
            string fileName = Application.persistentDataPath + "/savedObjects.txt";
            //XmlDocument xmlDocument = new XmlDocument();
            List<string> lines = new List<string>();

            foreach(GameObject go in spawnedObjects)
            {
                lines.Add(go.name);
                lines.Add(go.transform.position.ToString());
                lines.Add(go.transform.rotation.ToString());
            }

            File.WriteAllLines(fileName, lines.ToArray());

            Debug.Log("File Saved");
            displayTextTr.text = "File Saved";

            StartCoroutine(enableText(displayTextTr, timeToWait));

            //xmlDocument.Save(fileName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputR), inputButton, out bool isPressed, inputThreshold);

        ToggleTrainingMode();

        SaveObjects();

        if (!isMoving && isPressed)
        {
            StartMovement();
        } else if(isMoving && !isPressed)
        {
            EndMovement();
        } else if (isMoving && isPressed)
        {
            UpdateMovement();
        }
    }
}
