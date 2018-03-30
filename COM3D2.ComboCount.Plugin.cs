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
     PluginName("COM3D2.ComboCount.Plugin"), PluginVersion("0.0.0.1")]

    public class ComboCount : PluginBase
    {
        private XmlManager xmlManager;
        private Maid maid;
        private int iSceneLevel;
        private DanceMain danceMain = null;

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
        }

        public void Start()
        {
        }

        public void Update()
        {
            if(danceMain == null) return;

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
