using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class DialogResponseController : MonoBehaviour
{
    public GameObject responsePrefab;
    public List<GameObject> options;
    public GameObject selectedObjectPointer;

    public RectTransform responsesContainer;

    public CanvasGroup canvasGroup;

    public CanvasScaler scaler;
    
    private int _selectedOption = 0;
    private TextMeshProUGUI text;
    public int selectedOption
    {
        set
        {
            if (value < 0)
                value += options.Count;
            _selectedOption = value % options.Count;

            chooseOption(_selectedOption);
        }
        get
        {
            return _selectedOption;
        }

    }

    public event Action<int> optionChosen;

    public bool awaitingResponse = false;

    private bool openedLastFrame = false;

    private void Start()
    {
        optionChosen += DialogController.main.receiveResponse;
    }


    public void addResponse(string text)
    {
        GameObject addedObject = Instantiate(responsePrefab, responsesContainer.transform);
        options.Add(addedObject);
        addedObject.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void clearResponses()
    {
        for(int i = options.Count - 1; i >= 0; i--)
        {
            Destroy(options[i]);
            options.RemoveAt(i);
        }
    }

    

    public void setResponses(params string[] text)
    {
        clearResponses();
        for(int i = 0; i < text.Length; i++)
        {
            addResponse(text[i]);
        }
        openedLastFrame = true;
        //open();
        //Need delay from the openedOnFrame so that the pointer does not jump

        chooseOption(0);
        awaitingResponse = true;
    }

    public void chooseOption(int option)
    {
        if (options.Count == 0 || options.Count <= 0)
            return;
        option = Mathf.Abs(option) % options.Count;

        if (text != null)
        {
            text.fontStyle = FontStyles.Normal;
            text.characterSpacing = 0f;
            text.color = Color.white;
        }

        Vector3 finalPosition = default;
        //finalPosition.x = (-responsesContainer.rect.width) + responsesContainer.position.x;
        finalPosition.x = responsesContainer.position.x + (responsesContainer.rect.xMin + (responsesContainer.rect.width * .425f)) * (Screen.width/scaler.referenceResolution.x);

        finalPosition.y = options[option].transform.position.y;

        text = options[option].GetComponent<TextMeshProUGUI>();
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = -6.5f;
        text.color = new Color32(255, 221, 49, 255);
        
        selectedObjectPointer.transform.position = finalPosition;

    }

    public void open()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        selectedOption = 0;
    }

    public void close()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedOption--;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedOption++;
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.T))
        {
            if(awaitingResponse)
                makeChoice();
        }
    }

    private void FixedUpdate()
    {
        if (openedLastFrame)
        {
            openedLastFrame = false;
            chooseOption(selectedOption);
            open();
        }
    }

    public void makeChoice()
    {
        optionChosen?.Invoke(selectedOption);
        awaitingResponse = false;
    }

}
