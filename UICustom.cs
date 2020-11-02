using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CustomData
{
    // 슬롯에 들어갈 정보 (슬롯은 6칸으로 이루어져 있음)
    public GameObject slotobj;
    public Image slotimg;
    public Image slotface;
    public Text slotname;

    // 구매 여부에 따라 박스 활성화 여부가 결정됨
    public GameObject PriceBox;
    public Text PriceText;

    // 구매 했다면, 장착 / 비장착에 따라 텍스트를 달리함
    public GameObject StateBox;
    public Text StateText;

    // 슬롯을 누르면 패널을 띄우게 함
    public GameObject Panel;
    public Text Buytext;
    public Button BuyButton;
    public Button InfoButton;
}

public class UICustom : MonoBehaviour
{
    #region 의상 Data
    public TextAsset txt;
    private string[,] Sentence;
    public string[,] _Sentence
    {
        get { return Sentence; }
        set { Sentence = value; }
    }
    int lineSize, rowSize;
    #endregion

    [Header("Avater, My Character")]
    // 필드 상에 활성화 상태로 존재하는 내 캐릭터
    public SpriteRenderer MyAvatar;

    // 현재 아바타 데이터에 대한 정보
    public CustomTool[] CustomData;

    // 지금 까지 만들어둔 아바타 리소스는 여기에 다 넣음
    public Sprite[] CustomAvater;

    // 현재 장착중인 아바타의 넘버 번호
    private int EquipNumber;
    public int _EquipNumber
    {
        get { return EquipNumber; }
        set { EquipNumber = value; }
    }

    [Header("SlotData = MAX 6, UICLASSDATA")]
    // 노트 - 의상 탭의 페이지 할당 슬롯 (6칸)
    public CustomData[] CustomSlot;

    // 현재 페이지 정보
    [Header("Page")]
    public Text Page;
    public int Cur_page;
    public int Max_page;

    // 현재 적용되는 보유 효과 (코스튬 1벌에 절약율 2% 상승)
    [Header("EffectText")]
    public Text EffectPoint;
    private int CustomPoint;
    public int _CustomPoint
    {
        get { return CustomPoint; }
        set { CustomPoint = value; }
    }

    // 현재 장착 여부
    private bool isEquip = false;

    // 현재 선택된 코스튬의 번호
    private int Select_Num;

    // 최대 장착할 수 있는 갯수 (반드시 1 고정)
    private const int Max_Equip = 1;

    // 1 페이지에 들어갈 수 있는 최대 코스튬 수 (6칸이므로 반드시 6 고정)
    private const int MAX_SLOT = 6;

    private Color UnlockColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
    private Color UnEquipColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    private Color SelectColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    private Color EquipColor = Color.white;

    // 컴포넌트 연결
    DBManager DB;
    UIController UI;
    NoticeUI _notice;
    AudioManager _audio;

    void Awake()
    {
        // 스크립트 컴포넌트를 연결함
        DB = FindObjectOfType<DBManager>();
        UI = FindObjectOfType<UIController>();
        _notice = FindObjectOfType<NoticeUI>();
        _audio = FindObjectOfType<AudioManager>();

        #region 의상 데이터 사전화
        string currentText = txt.text.Substring(0, txt.text.Length - 1);
        string[] line = currentText.Split('\n');
        lineSize = line.Length;
        rowSize = line[0].Split('\t').Length;
        Sentence = new string[lineSize, rowSize];
        for (int i = 0; i < lineSize; i++)
        {
            string[] row = line[i].Split('\t');
            for (int j = 0; j < rowSize; j++)
            {
                Sentence[i, j] = row[j];
            }
        }
        #endregion

        #region 코스튬 데이터 넣기
        // Step 01. 미리 코스튬의 데이터를 채워넣음
        for (int i = 0; i < CustomData.Length; i++)
        {
            CustomData[i].img = CustomAvater[i];
            CustomData[i].name = Sentence[i, 1];
            CustomData[i].price = System.Convert.ToInt32(Sentence[i, 2]);
            CustomData[i].state_dir = PlayerPrefs.GetInt(("Custom" + i).ToString(), 0);
        }

        // 맨 처음 기본 코스튬은 처음 부터 활성화와 동시에 장착
        CustomData[0].state_dir = PlayerPrefs.GetInt(("Custom0").ToString(), 2);
        #endregion
    }

    void Start()
    {
        // 슬롯 데이터 초기화
        for (int i = 0; i < MAX_SLOT; i++)
        {
            CustomSlot[i].PriceBox.SetActive(false);
            CustomSlot[i].StateBox.SetActive(false);
            CustomSlot[i].slotobj.SetActive(false);
        }

        // 코스튬 데이터를 기반으로 UI의 첫 페이지에 보여줌
        for (int i = 0; i < MAX_SLOT; i++)
        {
            // 기본적인 데이터를 채워넣음
            CustomSlot[i].slotobj.SetActive(true);
            CustomSlot[i].slotimg.sprite = CustomData[i].img;
            CustomSlot[i].slotname.text = CustomData[i].name;

            // 장착 상태에 따라 슬롯 이미지의 색상 값과 객체 활성화 여부를 결정함
            switch(CustomData[i].state_dir)
            {
                case 0:
                    CustomSlot[i].PriceBox.SetActive(true);
                    CustomSlot[i].slotimg.color = UnlockColor;
                    CustomSlot[i].slotface.color = UnlockColor;
                    CustomSlot[i].PriceText.text = CustomData[i].price.ToString();
                    break;

                case 1:
                    CustomSlot[i].StateBox.SetActive(true);
                    CustomSlot[i].slotimg.color = UnEquipColor;
                    CustomSlot[i].slotface.color = UnEquipColor;
                    CustomSlot[i].StateText.text = "보유 중";
                    break;

                case 2:
                    CustomSlot[i].StateBox.SetActive(true);
                    CustomSlot[i].slotimg.color = EquipColor;
                    CustomSlot[i].slotface.color = EquipColor;
                    CustomSlot[i].StateText.text = "<color=yellow>현재 장착 중</color>";
                    break;
            }
        }

        // 의상 보유 효과 갱신 (물 획득량 증가)
        CustompointUpdate();

        // 페이지 자동 설정 및 초기화
        PageUpdate();

        // 버튼 onClick 추가 (메인 알림창)
        ButtonAction();

        // 보유 효과 합산 갱신
        DB.UpdateLoad();

        // PlayerPrefs을 통해 장착 중인 코스튬의 순번을 불러옴
        // 값이 없으면 순번 0, 평상복을 불러오게 됨
        EquipNumber = PlayerPrefs.GetInt("EquipCustom", 0);
        AvaterEquip(EquipNumber);
    }

    #region ICON CLICK
    // 코스튬 아이콘을 클릭하면 구매 하기 / 상세 정보 창을 띄움
    // 매개변수 Slotnum = 해당 슬롯의 순서 (0 ~ 5)
    public void IconClick(int Slotnum)
    {
        _audio.Play("pagemove");

        Select_Num = Slotnum;
        int number = Select_Num + ((Cur_page - 1) * MAX_SLOT);
        UpdateSlot();

        CustomSlot[Slotnum].slotimg.color = SelectColor;
        CustomSlot[Slotnum].slotface.color = SelectColor;

        CustomSlot[Slotnum].Panel.SetActive(true);
        switch(CustomData[number].state_dir)
        {
            case 0:
                CustomSlot[Slotnum].BuyButton.interactable = true;
                CustomSlot[Slotnum].Buytext.text = "구매하기";
                break;

            case 1:
                CustomSlot[Slotnum].BuyButton.interactable = true;
                CustomSlot[Slotnum].Buytext.text = "장착하기";
                break;

            case 2:
                CustomSlot[Slotnum].BuyButton.interactable = false;
                CustomSlot[Slotnum].Buytext.text = "장착 중";
                break;
        }

    }
    
    // 구매 버튼 클릭 (상태에 따라 장착하기가 될 수 있음)
    void BuyClick(int number)
    {
        switch (CustomData[number].state_dir)
        {
            // 미해금 상태(UNLOCK)
            case 0:
                // 구매에 필요한 미네랄을 갖고 있을 때에만 정상 처리
                if (DB._cur_price >= CustomData[number].price)
                {
                    DB._cur_price -= CustomData[number].price;
                    CustomData[number].state_dir = 1;
                    UpdateSlot();

                PlayerPrefs.SetInt(("Custom" + number).ToString(), CustomData[number].state_dir);
                PlayerPrefs.Save();
                SlotUpdate();
                UI.priceUpdate();
                CustompointUpdate();

                _audio.Play("itembuy");
                _notice.SUB("<color=yellow>" + CustomData[number].name + "</color> 의상을 구매했어요.");
                }
                else
                {
                    _audio.Play("error");
                    _notice.SUB("보유한 <color=yellow>미네랄</color>이 부족해요.");
                }
                break;

            // 해금 상태(UNEQUIP)
            case 1:
                AvaterEquip(number);
                _audio.Play("customequip");
                _notice.SUB("<color=yellow>" + CustomData[number].name + "</color> 의상을 착용했어요.");
                SlotUpdate();
                break;
        }
    }

    // 상세 정보
    void InfoClick()
    {

    }

    
    void ButtonAction()
    {
        // 확인을 누르게 되면 실행할 행동
        Action action = () =>
        {
            int slotnum = Select_Num + ((Cur_page - 1) * MAX_SLOT);
            BuyClick(slotnum);
        };

        for(int i = 0; i < CustomSlot.Length; i++)
        {
            // 구매 버튼의 onClick에 아래 기능을 추가함
            CustomSlot[i].BuyButton.onClick.AddListener(() =>
            {
                int slotnum = Select_Num + ((Cur_page - 1) * MAX_SLOT);
                switch (CustomData[slotnum].state_dir)
                {
                    case 0:
                        Popup popup = _notice.CreatePopup();
                        popup.Init(_notice.MainCanvas, "의상 구매", "<color=yellow>" + CustomData[slotnum].name + "</color> 의상을 구매합니까?", action);
                        break;

                    default:
                        BuyClick(slotnum);
                        break;
                }
            });

            // 상세보기 버튼의 onClick에 아래 기능을 추가함
            //CustomSlot[i].InfoButton.onClick.AddListener(() =>
            //{
            //
            //});
        }
    }
    #endregion

    #region PAGEMOVE
    public void ClickBefore()
    {
        _audio.Play("pagemove");

        if (Cur_page <= 1)
        {
            Cur_page = 1;
            return;
        }
        else
        {
            Cur_page--;
            Page.text = Cur_page + " / " + Max_page.ToString();
            // 문제 발생
            PageMove();
            UpdateSlot();
        }
    }

    public void ClickNext()
    {
        _audio.Play("pagemove");

        if (Cur_page >= Max_page)
        {
            Cur_page = Max_page;
            return;
        }
        else
        {
            Cur_page++;
            Page.text = Cur_page + " / " + Max_page.ToString();
            PageMove();
            UpdateSlot();
        }
    }

    // 페이지 이동 처리
    void PageMove()
    {
        // 슬롯 데이터 초기화
        for (int i = 0; i < MAX_SLOT; i++)
        {
            CustomSlot[i].PriceBox.SetActive(false);
            CustomSlot[i].StateBox.SetActive(false);
            CustomSlot[i].slotobj.SetActive(false);
        }

        SlotUpdate();
    }
    #endregion

    #region SYSTEM SETTING
    // 매개변수를 통한 아바타 장착
    // int number => 장착 중인 아바타 넘버
    public void AvaterEquip(int number)
    {
        // 현재 장착 중인 코스튬을 전부 찾아서 비장착 상태로 초기화 시킴
        for(int i = 0; i < CustomAvater.Length; i++)
        {
            if(CustomData[i].state_dir == 2)
            {
                CustomData[i].state_dir = 1;
            }
        }

        // 매개변수로 넘어온 아바타를 찾아서 장착하고 상태 값을 저장함
        MyAvatar.sprite = CustomAvater[number];
        CustomData[number].state_dir = 2;
        EquipNumber = number;
        PlayerPrefs.SetInt("EquipCustom", EquipNumber);
        SlotUpdate();
        UpdateSlot();
    }

    // 의상 보유 효과 갱신 (물 획득량 증가)
    void CustompointUpdate()
    {
        CustomPoint = 0;

        for (int i = 0; i < CustomData.Length; i++)
        {
            switch (CustomData[i].state_dir)
            {
                case 1:
                    CustomPoint += 3;
                    break;

                case 2:
                    CustomPoint += 3;
                    break;
            }
        }

        EffectPoint.text = "물 드롭 획득량 증가 : " + CustomPoint + "%".ToString();
    }

    // 페이지 자동 설정 및 초기화
    void PageUpdate()
    {
        // UI 표시 및 페이지 값 초기화
        // 물받이 상태 체크하여 EQUIP 상태일 때, Cur_Equip 를 1 올림
        Cur_page = 1;

        // MAX PAGE 값을 자동으로 계산함
        if (CustomAvater.Length % MAX_SLOT > 0)
        {
            Max_page = (CustomAvater.Length / MAX_SLOT) + 1;
        }
        else
        {
            Max_page = CustomAvater.Length / MAX_SLOT;
        }

        Page.text = Cur_page + " / " + Max_page.ToString();
    }

    // 슬롯 UI 정보 업데이트
    void SlotUpdate()
    {
        // 코스튬 데이터를 기반으로 UI의 첫 페이지에 보여줌
        for (int i = (Cur_page - 1) * MAX_SLOT; i < Cur_page * MAX_SLOT; i++)
        {
            int Slot_Count = i - ((Cur_page - 1) * MAX_SLOT);

            // 기본적인 데이터를 채워넣음
            // 슬롯 데이터 갱신 시, 게임에 등록된 코스튬 갯수를 초과하면 실행하지 않음
            if (CustomAvater.Length > i)
            {
                CustomSlot[Slot_Count].slotobj.SetActive(true);
                CustomSlot[Slot_Count].slotimg.sprite = CustomData[i].img;
                CustomSlot[Slot_Count].slotname.text = CustomData[i].name;

                // 장착 상태에 따라 슬롯 이미지의 색상 값과 객체 활성화 여부를 결정함
                switch (CustomData[i].state_dir)
                {
                    case 0:
                        CustomSlot[Slot_Count].PriceBox.SetActive(true);
                        CustomSlot[Slot_Count].StateBox.SetActive(false);
                        CustomSlot[Slot_Count].PriceText.text = CustomData[i].price.ToString();
                        break;

                    case 1:
                        CustomSlot[Slot_Count].PriceBox.SetActive(false);
                        CustomSlot[Slot_Count].StateBox.SetActive(true);
                        CustomSlot[Slot_Count].StateText.text = "보유 중";
                        break;

                    case 2:
                        CustomSlot[Slot_Count].PriceBox.SetActive(false);
                        CustomSlot[Slot_Count].StateBox.SetActive(true);
                        CustomSlot[Slot_Count].StateText.text = "<color=yellow>현재 장착 중</color>";
                        break;
                }
            }
        }
    }

    void UpdateSlot()
    {
        // 현재 열려있는 슬롯 패널을 닫고 새로운 패널을 띄움
        for (int i = 0; i < CustomSlot.Length; i++)
        {
            int number = i + ((Cur_page - 1) * MAX_SLOT);
            if (CustomAvater.Length > number)
            {
                // 슬롯의 아바타 컬러 변경
                switch (CustomData[number].state_dir)
                {
                    case 0:
                        CustomSlot[i].slotimg.color = UnlockColor;
                        CustomSlot[i].slotface.color = UnlockColor;
                        break;

                    case 1:
                        CustomSlot[i].slotimg.color = UnEquipColor;
                        CustomSlot[i].slotface.color = UnEquipColor;
                        break;

                    case 2:
                        CustomSlot[i].slotimg.color = EquipColor;
                        CustomSlot[i].slotface.color = EquipColor;
                        break;
                }

                // 열려 있는 패널창 닫음
                CustomSlot[i].Panel.SetActive(false);
            }
        }
    }
    #endregion
}
