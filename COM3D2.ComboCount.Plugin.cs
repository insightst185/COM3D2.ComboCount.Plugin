using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.ComboCount.Plugin
{
    [PluginFilter("COM3D2x64"),
     PluginName("COM3D2.ComboCount.Plugin"), PluginVersion("0.0.0.3")]

    public class ComboCount : PluginBase
    {
        private XmlManager xmlManager;
        private Maid maid;
        private int iSceneLevel;
        private DanceMain danceMain = null;
        private bool resetFlag;

        private string[] tagItemChanges =
        {
            "めくれスカート",
            "めくれスカート後ろ"
        };

        // presetリスト 3人分でいいか ダンス用だからね
        private const int MAX_LISTED_MAID = 3;
        private int[] presetPos = new int[MAX_LISTED_MAID];
        private int nowCombo = 0;
        private int latestCombo = 0;
        // どっかにありそうだけど見つからなかったのでくそコーディング
        private Dictionary<string, TBody.SlotID> slodIDdic = new Dictionary<string, TBody.SlotID>()
                                       {
    {"body",TBody.SlotID.body},
    {"head",TBody.SlotID.head},
    {"eye",TBody.SlotID.eye},
    {"hairF",TBody.SlotID.hairF},
    {"hairR",TBody.SlotID.hairR},
    {"hairS",TBody.SlotID.hairS},
    {"hairT",TBody.SlotID.hairT},
    {"wear",TBody.SlotID.wear},
    {"skirt",TBody.SlotID.skirt},
    {"onepiece",TBody.SlotID.onepiece},
    {"mizugi",TBody.SlotID.mizugi},
    {"panz",TBody.SlotID.panz},
    {"bra",TBody.SlotID.bra},
    {"stkg",TBody.SlotID.stkg},
    {"shoes",TBody.SlotID.shoes},
    {"headset",TBody.SlotID.headset},
    {"glove",TBody.SlotID.glove},
    {"accHead",TBody.SlotID.accHead},
    {"hairAho",TBody.SlotID.hairAho},
    {"accHana",TBody.SlotID.accHana},
    {"accHa",TBody.SlotID.accHa},
    {"accKami_1_",TBody.SlotID.accKami_1_},
    {"accMiMiR",TBody.SlotID.accMiMiR},
    {"accKamiSubR",TBody.SlotID.accKamiSubR},
    {"accNipR",TBody.SlotID.accNipR},
    {"HandItemR",TBody.SlotID.HandItemR},
    {"accKubi",TBody.SlotID.accKubi},
    {"accKubiwa",TBody.SlotID.accKubiwa},
    {"accHeso",TBody.SlotID.accHeso},
    {"accUde",TBody.SlotID.accUde},
    {"accAshi",TBody.SlotID.accAshi},
    {"accSenaka",TBody.SlotID.accSenaka},
    {"accShippo",TBody.SlotID.accShippo},
    {"accAnl",TBody.SlotID.accAnl},
    {"accVag",TBody.SlotID.accVag},
    {"kubiwa",TBody.SlotID.kubiwa},
    {"megane",TBody.SlotID.megane},
    {"accXXX",TBody.SlotID.accXXX},
    {"chinko",TBody.SlotID.chinko},
    {"chikubi",TBody.SlotID.chikubi},
    {"accHat",TBody.SlotID.accHat},
    {"kousoku_upper",TBody.SlotID.kousoku_upper},
    {"kousoku_lower",TBody.SlotID.kousoku_lower},
    {"seieki_naka",TBody.SlotID.seieki_naka},
    {"seieki_hara",TBody.SlotID.seieki_hara},
    {"seieki_face",TBody.SlotID.seieki_face},
    {"seieki_mune",TBody.SlotID.seieki_mune},
    {"seieki_hip",TBody.SlotID.seieki_hip},
    {"seieki_ude",TBody.SlotID.seieki_ude},
    {"seieki_ashi",TBody.SlotID.seieki_ashi},
    {"accNipL",TBody.SlotID.accNipL},
    {"accMiMiL",TBody.SlotID.accMiMiL},
    {"accKamiSubL",TBody.SlotID.accKamiSubL},
    {"accKami_2_",TBody.SlotID.accKami_2_},
    {"accKami_3_",TBody.SlotID.accKami_3_},
    {"HandItemL",TBody.SlotID.HandItemL},
    {"underhair",TBody.SlotID.underhair},
    {"moza",TBody.SlotID.moza},
    {"end",TBody.SlotID.end},
                                         };

        private void SetPreset(Maid maid, string fileName)
        {
            var preset = GameMain.Instance.CharacterMgr.PresetLoad(Path.Combine(Path.GetFullPath(".\\") + "Preset", fileName));
            GameMain.Instance.CharacterMgr.PresetSet(maid, preset);
        }

        public void Awake()
        {
        }

        public void OnDestroy()
        {

        }

        public void OnLevelWasLoaded(int level)
        {
            iSceneLevel = level;

            danceMain = (DanceMain)FindObjectOfType(typeof(DanceMain));
            if(danceMain == null) return;

            Initialization();
        }

        //初期化処理
        private void Initialization()
        {
            xmlManager = new XmlManager();
            latestCombo = 0;
            for(int maidNo = 0; maidNo < MAX_LISTED_MAID; maidNo++){
                presetPos[maidNo] = 0;
            }
            resetFlag = true;
        }

        public void Start()
        {
        }

        public void Update()
        {
            if(danceMain == null) return;

            if(resetFlag){
                for(int maidNo = 0; maidNo < MAX_LISTED_MAID; maidNo++){
                    maid = GameMain.Instance.CharacterMgr.GetMaid(maidNo);
                    if(maid == null) break;
                    maid.body0.SetMaskMode(TBody.MaskMode.None);
                    resetFlag = false;
                }
            }

            nowCombo = Score_Mgr.Instance.GetCombo(DanceBattle_Mgr.CharaType.Player);

            if(latestCombo == nowCombo) return;

            latestCombo = nowCombo;

            for(int maidNo = 0; maidNo < MAX_LISTED_MAID; maidNo++){

                if(nowCombo != xmlManager.GetCombo(maidNo,presetPos[maidNo])) continue;

                string fileName = xmlManager.GetFileName(maidNo,presetPos[maidNo]);
                if(fileName == null) continue;

                maid = GameMain.Instance.CharacterMgr.GetMaid(maidNo);
                if (maid != null) {
                    String extent = Path.GetExtension(fileName);
                    if(extent.Equals(".preset")){
                           SetPreset(maid,fileName);
                    }
                    else if(extent.Equals(".menu")){
                        maid.SetUpModel(fileName);
//                                    Menu.ProcScript(maid,fileName,false);
//                                    maid.AllProcPropSeqStart();
                    }
                    else{
                        maid.body0.SetMask(slodIDdic[fileName],false);
                    }
                    presetPos[maidNo]++;
                }
            }

        }

        //------------------------------------------------------xml--------------------------------------------------------------------
        private class XmlManager
        {
            private string xmlFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Config\ComboCount.xml";
            private XmlDocument xmldoc = new XmlDocument();
            private List<string>[] listFileName = new List<string>[MAX_LISTED_MAID];
            private List<int>[] listCombo = new List<int>[MAX_LISTED_MAID];
            
            public XmlManager()
            {
                for(int i=0; i < MAX_LISTED_MAID; i++){
                    listFileName[i] = new List<string>();
                    listCombo[i] = new List<int>();
                }
                
                try{
                    InitXml();
                }
                catch(Exception e){
                    Debug.LogError("ComboCount.Plugin:" + e.Source + e.Message + e.StackTrace);
                }
            }

            private void InitXml()
            {
                xmldoc.Load(xmlFileName);
                // MenuList
                XmlNodeList menuList = xmldoc.GetElementsByTagName("MenuList");
                foreach (XmlNode presetFile in menuList)
                {
                    int maidNo = Int32.Parse(((XmlElement)presetFile).GetAttribute("maidNo"));
                    XmlNodeList Menus = ((XmlElement)presetFile).GetElementsByTagName("Menu");
                    foreach (XmlNode MenuTag in Menus){
                        listFileName[maidNo].Add(((XmlElement)MenuTag).GetAttribute("File"));
                        listCombo[maidNo].Add(Int32.Parse(((XmlElement)MenuTag).GetAttribute("Combo")));
                    }
                }
            }

            public string GetFileName(int no,int pos){
                string[] fileNames = listFileName[no].ToArray();
                if (fileNames.Length <= pos)
                {
                    return null;
                }
                else{
                    return fileNames[pos];
                }
            }

            public int GetCombo(int no,int pos){
                int[] GetCombos = listCombo[no].ToArray();
                if (GetCombos.Length <= pos)
                {
                    return -1;
                }
                else{
                    return GetCombos[pos];
                }
            }

        }
    }
}
