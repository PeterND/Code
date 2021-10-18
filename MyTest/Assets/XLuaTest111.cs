using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XLua
{
    public class XLuaTest111 : MonoBehaviour
    {
        LuaEnv myluaEnv;
        private void Awake()
        {
            myluaEnv = new LuaEnv();
        }

        // Start is called before the first frame update
        void Start()
        {
            myluaEnv.DoString("CS.UnityEngine.Debug.Log('hello world')");//DoString内的参数时合法的Lua代码即可。
            myluaEnv.Dispose();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
