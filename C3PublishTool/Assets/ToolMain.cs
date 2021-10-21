using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System;



public class ToolMain : MonoBehaviour
{
    
    const string IOS_FLAG = "ios";
    const string WIN_FLAG = "win";
    const string ANDROID_FLAG = "android";
    const string Sperator_Flag = " ";

    enum ToolState
    {
        Wait,
        Runing,
        End,
    }
    struct RunParam
    {
        public string strPlatform;
        public string strIsAdd;
        public string strIsDealPack;
        public string tempOuput;
        public string outputFloderName;
        public string executeEnvPath;
    }
    Button m_quitBtn;
    Button m_beginBtn;
    Toggle m_isAddToggle;
    Toggle m_isDealPackToggle;
    Dropdown m_platformDropDown;
    Text m_beginBtnName;

    string m_c3PackResPath;
    

    Process m_currentProcess;
    Stopwatch m_stopwatch = new Stopwatch();
    bool m_bIsOnBeginPackRes;  //是否正在执行Cmd;
    IniConfig m_iniConfig;
    RunParam m_runParam;

    ToolState m_toolState;

    bool m_isCanUse;
    void Start()
    {
        m_isCanUse = false;
        InitUI();
        m_toolState = ToolState.Wait;
        string dataPath = Application.dataPath.Replace('/','\\');

        m_c3PackResPath = dataPath + "\\..\\C3PackRes2\\";
        if (!Directory.Exists(m_c3PackResPath))
        {
            UnityEngine.Debug.LogAssertion("缺少C3PackRes2目录无法使用该工具");
            return;
        }
        m_iniConfig = new IniConfig(m_c3PackResPath + "config.ini");
        m_isCanUse = true;  

    }

    void InitUI()
    {
        Transform[] allChildTransform = this.gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform childTransform in allChildTransform)
        {
            if (childTransform.gameObject.name == "QuitBtn")
            {
                m_quitBtn = childTransform.gameObject.GetComponent<Button>();
                m_quitBtn.onClick.AddListener(OnClickQuitBtn);
            }
            else if (childTransform.gameObject.name == "BeginBtn")
            {
                m_beginBtn = childTransform.gameObject.GetComponent<Button>();
                m_beginBtn.onClick.AddListener(OnClickBeginBtn);
            }
            else if (childTransform.gameObject.name == "IsDealPack")
            {
                m_isDealPackToggle = childTransform.gameObject.GetComponent<Toggle>();
            }
            else if (childTransform.gameObject.name == "IsAdd")
            {
                m_isAddToggle = childTransform.gameObject.GetComponent<Toggle>();
            }
            else if (childTransform.gameObject.name == "PlatformDropDown")
            {
                m_platformDropDown = childTransform.gameObject.GetComponent<Dropdown>();
            }
            else if (childTransform.gameObject.name == "BeginBtnName")
            {
                m_beginBtnName = childTransform.gameObject.GetComponent<Text>();
            }

        }
        m_beginBtnName.text = "开始打包";
        m_beginBtnName.color = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
         if (m_toolState == ToolState.End)
         {
            m_beginBtnName.text = "开始打包";
            m_beginBtnName.color = Color.green;
            m_toolState = ToolState.Wait;
         }
    }

    void OnClickQuitBtn()
    {
        if (m_currentProcess != null)
        {
            m_currentProcess.Kill();
        }

        Application.Quit();

    }

    void OnClickBeginBtn()
    {
        if (!m_isCanUse)
        {
            UnityEngine.Debug.LogAssertion("缺少C3PackRes2目录无法使用该工具");
            return;
        }

        if (m_currentProcess != null)
        {
             m_currentProcess.Kill();
        }
        else
        {

            InitRunParam();
            Thread newThread = new Thread(new ThreadStart(OnBeginPackRes));
            newThread.Start();
            m_bIsOnBeginPackRes = true;
            m_beginBtnName.color = Color.red;
            m_beginBtnName.text = "停止打包";
            m_toolState = ToolState.Runing;
        }

    }

    void InitRunParam()
    {
        m_runParam.strPlatform = m_platformDropDown.options[m_platformDropDown.value].text;
        m_runParam.strIsAdd = m_isAddToggle.isOn ? "1" : "0";
        m_runParam.strIsDealPack = m_isDealPackToggle.isOn ? "1" : "0";
        if ("1" == m_runParam.strIsAdd)
        {
            m_runParam.executeEnvPath = m_iniConfig.ReadValue("OriginalResource", "Addition_Res_Env");
        }
        else
        {
            m_runParam.executeEnvPath = m_iniConfig.ReadValue("OriginalResource", "Full_Res_Env");
        }

        m_runParam.tempOuput = "..\\" + m_runParam.strPlatform;
        m_runParam.tempOuput = m_isAddToggle.isOn ? m_runParam.tempOuput + "_addition" : m_runParam.tempOuput + "_full";
        string[] arrayOuputPathName = {m_runParam.tempOuput,DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString(), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString(), m_iniConfig.ReadValue("Version", "VersionCode") };
        m_runParam.outputFloderName = string.Join("_", arrayOuputPathName);
        m_runParam.tempOuput = m_runParam.executeEnvPath + "\\" + m_runParam.outputFloderName;
        string initLog = string.Format("PackRes Start InitParam Platform({0}),IsAdd({1}),IsDealPack({2}),excuteEnvPath({3}),tempOuput({4})",
            m_runParam.strPlatform, m_runParam.strIsAdd, m_runParam.strIsDealPack, m_runParam.executeEnvPath, m_runParam.tempOuput);
        RecordLog(initLog);

    }

    void OnBeginPackRes()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        InitRunParam();
        bool bIsSuc = true;
        if (!ExecuteResConvert())
        {
            bIsSuc = false;
        }
        else if (!ExecuteRecordVersion())
        {
            bIsSuc = false;
        }
        else if (!ExecuteEncrypt())
        {
            bIsSuc = false;
        }
        else if (!ExecutePackRes())
        {
            bIsSuc = false;
        }
        stopwatch.Stop();
        if (bIsSuc)
        {
            RecordLog(string.Format("PackRes Finish Cost Second{0}", stopwatch.Elapsed.TotalSeconds));
        }
        else
        {
            RecordLog(string.Format("PackRes Finish Cost Second{0}", stopwatch.Elapsed.TotalSeconds),LogType.Error);
        }
        m_toolState = ToolState.End;
    }

    bool ExecuteBat(string batFilePath,string batFileName,string args)
    {
        string fullBatPath = batFilePath + "\\"+ batFileName;
        if (!System.IO.File.Exists(fullBatPath))
        {
            RecordLog("bat文件不存在：" + fullBatPath,LogType.Error);
        }
        else
        {
            RecordLog(string.Format("bat文件执行开始：路径({0},参数({1}))", fullBatPath,args));
            m_stopwatch.Reset();
            m_stopwatch.Start();
            m_currentProcess = ExecCmdUtility.CreateShellExProcess(batFileName, args, batFilePath);
            if (m_currentProcess != null)
            {
                m_currentProcess.WaitForExit();
            }
            if (m_currentProcess.ExitCode != 0)
            {
                RecordLog(string.Format("bat文件执行失败：路径({0},参数({1},返回值{2})" , fullBatPath, args, m_currentProcess.ExitCode), LogType.Error);

            }
            else
            {
                m_stopwatch.Stop();
                RecordLog("bat文件执行结束：" + fullBatPath + "耗时:" + m_stopwatch.Elapsed.TotalSeconds.ToString());
                return true;
            }

        }
        return false;
    }

    public static FileInfo[] ReadFolder(string folderPath, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
    {
        //获取指定路径下面的所有资源文件  
        if (Directory.Exists(folderPath))
        {
            DirectoryInfo direction = new DirectoryInfo(folderPath);
            FileInfo[] files = direction.GetFiles(searchPattern, searchOption);
            return files;
        }

        else return null;

    }
    public delegate bool DelCondition<FileInfo>(FileInfo fileInfo);
    public static FileInfo[] GetFiles(string folderPath, DelCondition<FileInfo> condition)
    {
        FileInfo[] temp = ReadFolder(folderPath);
        List<FileInfo> fileInfos = new List<FileInfo>();

        for (int i = 0; i < temp.Length; i++)
        {
            if (!condition(temp[i])) continue;
            fileInfos.Add(temp[i]);
        }

        return fileInfos.ToArray();
    }


    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);   //计算指定Stream 对象的哈希值  
            file.Close();

            StringBuilder Ac = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                Ac.Append(retVal[i].ToString("x2"));
            }
            return Ac.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }


    void CreateOrOPenFile(string path, string name, string info)
    {
        StreamWriter sw;
        FileInfo fi = new FileInfo(path + "//" + name);
        sw = fi.CreateText();        //直接重新写入
        sw = fi.AppendText();          //在原文件后面追加内容
        sw.WriteLine(info);
        sw.Close();
        sw.Dispose();
    }

    bool ExecuteResConvert()
    {
        bool bRes = false;
        if (m_runParam.strPlatform == IOS_FLAG || m_runParam.strPlatform == ANDROID_FLAG)
        {
            File.Copy(m_c3PackResPath + "ResConvert/ResConvert.bat", m_runParam.executeEnvPath+ "/ResConvert.bat", true);
            File.Copy(m_c3PackResPath + "ResConvert/Tools/PVRTexTool.exe", m_runParam.executeEnvPath + "/PVRTexTool.exe", true);
            string fullEnvPath = m_iniConfig.ReadValue("OriginalResource", "Full_Res_Env");
            string[] arrayParam = { m_runParam.strPlatform,
                m_runParam.outputFloderName ,
                m_runParam.strIsAdd,
                fullEnvPath,
                m_runParam.strIsDealPack,
                m_runParam.executeEnvPath,
                m_c3PackResPath + "ResConvert\\Tools"};
            string args = string.Join(Sperator_Flag, arrayParam);
            bRes =ExecuteBat(m_runParam.executeEnvPath,"ResConvert.bat", args);
            File.Delete(m_runParam.executeEnvPath + "/ResConvert.bat");
            if ("1" == m_runParam.strIsAdd)
            {
                File.Delete(m_runParam.executeEnvPath + "/PVRTexTool.exe");
            }
        }
        else if (m_runParam.strPlatform == WIN_FLAG)
        {
           if ("1" == m_runParam.strIsAdd)
            {
                string firstParam = m_runParam.executeEnvPath + "/*.*";
                string secondParam  = m_runParam.tempOuput + "/*.*";
                string[] arrayParam = { firstParam, secondParam };
                string args = string.Join(Sperator_Flag, arrayParam);
                bRes = ExecuteBat(m_c3PackResPath, "CopyDir.bat",args);
            }
        }
        return bRes;
    }

    bool ExecuteRecordVersion()
    {
        string versionDat = m_iniConfig.ReadValue("Version", "VersionDat");
        string[] arrayParam = { m_runParam.strPlatform,
            m_runParam.executeEnvPath,
            m_runParam.outputFloderName,
            versionDat,
            m_c3PackResPath + "\\ResConvert\\Tools"};
        string args = string.Join(Sperator_Flag, arrayParam);
        return ExecuteBat(m_c3PackResPath + "RecordPackInfo", "RecordPackInfo.bat", args);
    }

    bool ExecuteEncrypt()
    {
        string isIniToDat = m_iniConfig.ReadValue("Encrypt", "ini_to_dat");
        string projectType = m_iniConfig.ReadValue("Project", "ProjectType");
        string fullEnvPath = m_iniConfig.ReadValue("OriginalResource", "Full_Res_Env");
        string[] arrayParam = { m_runParam.tempOuput,
            m_runParam.executeEnvPath,
            isIniToDat,
            projectType,
            fullEnvPath };
        string args = string.Join(Sperator_Flag, arrayParam);
        return ExecuteBat(m_c3PackResPath + "Encrypt", "EncryptMain.bat", args);
    }

    bool ExecutePackRes()
    {
        bool bRes = false;
        string unpackPath = m_iniConfig.ReadValue("Pack", "Unpack_Path");
        string packPath = m_iniConfig.ReadValue("Pack", "Packed_Path");
        string fullEnvPath = m_iniConfig.ReadValue("OriginalResource", "Full_Res_Env");
        string resConvertFinishTempOuputPath = m_iniConfig.ReadValue("Pack", "Tmp_Output_Path");
        string[] arrayParam = { unpackPath,
            packPath,
            m_runParam.strPlatform,
            m_runParam.strIsAdd,
            fullEnvPath,
            m_runParam.executeEnvPath,
            m_runParam.tempOuput,
            resConvertFinishTempOuputPath };
        string args = string.Join(Sperator_Flag, arrayParam);
        bRes = ExecuteBat(m_c3PackResPath + "C3PackRes", "C3PackRes.bat", args);
        RecordLog(string.Format("PackRes Finish UnPackPath({0}) PackPath({1})", unpackPath, packPath));

        return bRes;
    }


    void RecordResConvertFileInfo()
    {
        FileInfo fi = new FileInfo("D:/TESTMD5.txt");
        StreamWriter sw;
        sw = fi.AppendText();
        FileInfo[] allFile = ReadFolder("D:/Code/env_mini/");
        foreach (FileInfo fileInfo in allFile)
        {
            string md5 = GetMD5HashFromFile(fileInfo.FullName);

            sw.WriteLine(md5 + " ");
        }
    }

    void RecordLog(string logContent, LogType logType = LogType.Log)
    {
        if (logType == LogType.Log)
        {
            UnityEngine.Debug.Log(logContent);
        }
        else
        {
            UnityEngine.Debug.LogError(logContent);
        }
        string logPath = m_c3PackResPath + m_iniConfig.ReadValue("Log", "LogFileName");
        FileInfo fileInfo = new FileInfo(logPath);
        StreamWriter sw;
        if (File.Exists(logPath))
        {
            sw = fileInfo.AppendText();
        }
        else
        {
            sw = fileInfo.CreateText();
        }
        string[] arrayTime = { DateTime.Now.Year.ToString(),DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString(), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString()};

        logContent = string.Join("-", arrayTime) + Sperator_Flag + logContent ;
        sw.WriteLine(logContent);
        sw.Close();
    }
}

