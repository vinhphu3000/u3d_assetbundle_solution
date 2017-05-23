using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Animations;

public class AnimatorControllerGen : EditorWindow
{
    [MenuItem("TOOLS/make new animator controller")]
    static void Init()
    { 
        EditorWindow.GetWindow<AnimatorControllerGen>();
    }

    void OnEnable()
    {
         minSize = new Vector2(750, 320);  
    }

    void OnDisable()
    {
        //save?
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
     

    public class AniStateAttribute
    {
        public string name;
        public string describe;
        public bool isOne = false;
        public AnimationClip clip1;
        public AnimationClip clip2;
    }

    List<AniStateAttribute> allStateAtt = new List<AniStateAttribute>();
    Dictionary<string,AnimationClip> clips = new Dictionary<string,AnimationClip>();

    void InitStateAttInfos()
    {
        allStateAtt.Add(new AniStateAttribute { name = "chuchang", isOne = true, describe = "出场动作" });
        allStateAtt.Add(new AniStateAttribute { name = "siwang", isOne = true, describe = "死亡动作" }); 
        allStateAtt.Add(new AniStateAttribute { name = "jidao", isOne = true, describe = "击倒动作" });
        allStateAtt.Add(new AniStateAttribute { name = "qishen", isOne = true, describe = "起身动作" });

        allStateAtt.Add(new AniStateAttribute { name = "shangdaiji_xiadaiji", isOne = true, describe = "空闲_空闲 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "shangdaiji_qianpaobu", isOne = false, describe = "空闲_前跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "shangdaiji_zuopaobu", isOne = false, describe = "空闲_左跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "shangdaiji_youpaobu", isOne = false, describe = "空闲_右跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "shangdaiji_houpaobu", isOne = false, describe = "空闲_后跑步 动作" });

        allStateAtt.Add(new AniStateAttribute { name = "gongji1_xiadaiji", isOne = false, describe = "攻击1_空闲 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "gongji2_xiadaiji", isOne = false, describe = "攻击2_空闲 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "gongji1_qianpaobu", isOne = false, describe = "攻击1_前跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "gongji2_qianpaobu", isOne = false, describe = "攻击2_前跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "gongji1_zuopaobu", isOne = false, describe = "攻击1_左跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "gongji2_zuopaobu", isOne = false, describe = "攻击2_左跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "gongji1_youpaobu", isOne = false, describe = "攻击1_右跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "gongji2_youpaobu", isOne = false, describe = "攻击2_右跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "gongji1_houpaobu", isOne = false, describe = "攻击1_后跑步 动作" });
        allStateAtt.Add(new AniStateAttribute { name = "gongji2_houpaobu", isOne = false, describe = "攻击2_后跑步 动作" });

        //allStateAtt.Add(new AniStateAttribute { name = "jineng1_qianpaobu", isOne = false, describe = "技能1_前跑步 动作" });
        //allStateAtt.Add(new AniStateAttribute { name = "jineng2_qianpaobu", isOne = false, describe = "技能1_前跑步 动作" });
        //allStateAtt.Add(new AniStateAttribute { name = "jineng1_zuopaobu", isOne = false, describe = "技能1_左跑步 动作" });
        //allStateAtt.Add(new AniStateAttribute { name = "jineng2_zuopaobu", isOne = false, describe = "技能1_左跑步 动作" });
        //allStateAtt.Add(new AniStateAttribute { name = "jineng1_youpaobu", isOne = false, describe = "技能1_右跑步 动作" });
        //allStateAtt.Add(new AniStateAttribute { name = "jineng2_youpaobu", isOne = false, describe = "技能1_右跑步 动作" });
        //allStateAtt.Add(new AniStateAttribute { name = "jineng1_houpaobu", isOne = false, describe = "技能1_后跑步 动作" });
        //allStateAtt.Add(new AniStateAttribute { name = "jineng2_houpaobu", isOne = false, describe = "技能2_后跑步 动作" });

        //allStateAtt.Add(new AniStateAttribute { name = "jineng1_xiadaiji", isOne = false, describe = "技能1_空闲 动作" });
        //allStateAtt.Add(new AniStateAttribute { name = "jineng2_xiadaiji", isOne = false, describe = "技能2_空闲 动作" }); 
    }
    /*
     *   if (clips.Count == 0)
        {
            GUILayout.Label("select animation clips");
            if (GUILayout.Button(@"绾喛顓婚柅澶嬪"))
            {
                var selectClips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Deep);
                if (selectClips != null && selectClips.Length > 0)
                {
                    clips.Clear();
                    foreach (var v in selectClips)
                    {
                        clips.Add(v as AnimationClip);
                    }
                }
            }
        }
        else
        {
            if (GUILayout.Button("Clear all clips"))
            {
                clips.Clear();
                Selection.objects = null;
            }
        }
     */
    Vector2 scrollPosition;
    Vector2 scrollPosition1;
    void ShowStateClipInfos()
    {
        if (allStateAtt.Count == 0)
        {
            InitStateAttInfos();
        }

        GUILayout.BeginVertical();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (var v in allStateAtt)
        {
            GUI.color = Color.white;
            GUILayout.Label("===================================================================");
            GUI.color = Color.green;
            GUILayout.Label("动作名字:" + v.name);
            GUILayout.Label("动作说明:" + v.describe);
            v.isOne = EditorGUILayout.Toggle("is one:", v.isOne);
           GUILayout.BeginHorizontal(); 
            if (v.isOne)
            {
                v.clip1 = EditorGUILayout.ObjectField("动作资源:", v.clip1, typeof(AnimationClip), false) as AnimationClip;
            }
            else
            {
                v.clip1 = EditorGUILayout.ObjectField("上半身动作资源", v.clip1, typeof(AnimationClip), false) as AnimationClip;
                v.clip2 = EditorGUILayout.ObjectField("下半身动作资源", v.clip2, typeof(AnimationClip), false) as AnimationClip;
            }
           GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUI.color = Color.white;  
    }

    void ShowAniClipBaseArea()
    {
        ////设置clip区域  
        //Rect sfxPathRect = EditorGUILayout.GetControlRect(); 
        ////  DragAndDrop.objectReferences;
        //if (sfxPathRect.Contains(Event.current.mousePosition))  //(Event.current != null || Event.current.type == EventType.DragExited) && 
        //{
        //    Debug.LogError("dssafasdf");
        //} 
         
        GUI.color = Color.gray;
     //   GUILayout.BeginVertical();
        foreach (var v in clips)
        {
            EditorGUILayout.ObjectField(v.Value, typeof(AnimationClip),false);
        } 

     //   GUILayout.EndHorizontal();

        GUI.color = Color.white;
        if (clips.Count == 0)
        {
            if (GUILayout.Button("处理选中动画"))
            {
                var selectClips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Deep);
                if (selectClips != null && selectClips.Length > 0)
                {
                    clips.Clear();
                    foreach (var v in selectClips)
                    {
                        clips[(v as AnimationClip).name] = v as AnimationClip;
                    }
                }

                foreach (var v in allStateAtt)
                {
                    v.clip1 = null;
                    v.clip2 = null;
                }
            }
        }
        else
        {
            if (GUILayout.Button("清除所有动画"))
            {
                clips.Clear();
            }
        }  
    }

    void AutoSetStateClip()
    {
        if (clips.Count == 0)
            return;
        GUILayout.BeginVertical();
        scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1);
        if (GUILayout.Button("自动设置状态动画"))
        {
            foreach (var v in allStateAtt)
            {
                AnimationClip oneClip;
                clips.TryGetValue(v.name, out oneClip);
                if (oneClip != null && oneClip.name!="shangdaiji_xiadaiji")
                {
                    v.clip1 = oneClip;
                    v.isOne = true;
                }
                else
                {
                    string[] strs = v.name.Split(new char[] { '_' });
                    if (strs.Length == 2)
                    {
                        AnimationClip clip1;
                        AnimationClip clip2;
                        //得到上半身动作
                        clips.TryGetValue(strs[0], out clip1);
                        if (clip1 == null && strs[0] == "shangdaiji")
                        {
                            clips.TryGetValue("shangdaiji_xiadaiji", out clip1);
                        }
                        if (clip1 == null)
                        {
                            error += "动画设置出错,没有找到动作1:" + strs[0];
                        }
                        //得到下半身动作
                        clips.TryGetValue(strs[1], out clip2);
                        if (clip2 == null && strs[1] == "xiadaiji")
                        {
                            clips.TryGetValue("shangdaiji_xiadaiji", out clip2);
                        } 
                        if (clip2 == null)
                        {
                            error += "动画设置出错,没有找到动作2:" + strs[1]; 
                        } 
                        v.isOne = false;
                        v.clip1 = clip1;
                        v.clip2 = clip2;
                    }
                }
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void GenAniCtr()
    {
        if (GUILayout.Button("生成新的AnimatorController资源", new GUILayoutOption[] { GUILayout.Height(100) }))
        {
            string path = AssetDatabase.GetAssetPath(animatorCtrAsset);
            AssetDatabase.DeleteAsset(path);
            animatorCtrAsset = new AnimatorController();
            AssetDatabase.CreateAsset(animatorCtrAsset, path);
 

            int c = animatorCtrAsset.layers.Length;
            for (int i = 1; i < c; i++)
                animatorCtrAsset.RemoveLayer(i);

            var layers = new AnimatorControllerLayer[3];

            //生成common layer     
            layers[0] = new AnimatorControllerLayer();
            layers[0].iKPass = false;
            layers[0].defaultWeight = 1;
            layers[0].name = "common";
            layers[0].avatarMask = null;
            layers[0].blendingMode = AnimatorLayerBlendingMode.Override;
            layers[0].syncedLayerAffectsTiming = false;
            layers[0].stateMachine = new AnimatorStateMachine();
            layers[0].stateMachine.name = "common";  
            var commoneNullState = layers[0].stateMachine.AddState("null"); 

            //生成body_up layer 
            layers[1] = new AnimatorControllerLayer();
            layers[1].iKPass = false;
            layers[1].avatarMask = bodyUpMask;
            layers[1].defaultWeight = 1;
            layers[1].name = "blend_body_up";
            layers[1].blendingMode = AnimatorLayerBlendingMode.Override;
            layers[1].syncedLayerAffectsTiming = false;
            layers[1].stateMachine = new AnimatorStateMachine(); 
            layers[1].stateMachine.hideFlags = HideFlags.None;
            layers[1].stateMachine.name = "blend_body_up";
            var bodyUpLayerNullState = layers[1].stateMachine.AddState("null"); 
            Dictionary<string,AnimatorState> bodyUpStates = new Dictionary<string,AnimatorState>();
            bodyUpStates.Add("null",bodyUpLayerNullState); 

            //生成body_down layer 
            layers[2] = new AnimatorControllerLayer(); 
            layers[2].iKPass = false;
            layers[2].defaultWeight = 1;
            layers[2].name = "blend_body_down";
            layers[2].avatarMask = bodyDownMask;
            layers[2].blendingMode = AnimatorLayerBlendingMode.Override;
            layers[2].stateMachine = new AnimatorStateMachine(); 
            layers[2].stateMachine.name = "blend_body_down"; 
            var bodyDownLayerNullState = layers[2].stateMachine.AddState("null");
            layers[2].syncedLayerAffectsTiming = false; 
            Dictionary<string,AnimatorState> bodyDownStates = new Dictionary<string,AnimatorState>();
            bodyDownStates.Add("null",bodyDownLayerNullState);
               
            animatorCtrAsset.parameters = null;
            //设置3个开关变量
            animatorCtrAsset.AddParameter("stop_common", AnimatorControllerParameterType.Bool);
            animatorCtrAsset.AddParameter("stop_body_up", AnimatorControllerParameterType.Bool);
            animatorCtrAsset.AddParameter("stop_body_down", AnimatorControllerParameterType.Bool); 
            //设置3个转换变量
            animatorCtrAsset.AddParameter("common_trans", AnimatorControllerParameterType.Int);
            animatorCtrAsset.AddParameter("body_up_trans", AnimatorControllerParameterType.Int);
            animatorCtrAsset.AddParameter("body_down_trans", AnimatorControllerParameterType.Int);

            List<AnimatorControllerParameter> paras = new List<AnimatorControllerParameter>();
            paras.AddRange(animatorCtrAsset.parameters);

            foreach (var v in allStateAtt)
            {
                //如果是一个动画 则放在common层
                if (v.isOne && v.clip1 != null)
                {
                    var state = layers[0].stateMachine.AddState(v.name, layers[0].stateMachine.entryPosition +
                        new Vector3(400 + 60 * Mathf.Sin(layers[1].stateMachine.states.Length * 20f), -400 + 80 * layers[0].stateMachine.states.Length, 0));
                    var trans = state.AddTransition(commoneNullState);
                    trans.AddCondition(AnimatorConditionMode.If, 1, "stop_common");
                    state.motion = v.clip1;

                    //记录动画时长
                    string clipLenName = v.name;
                    var strs = v.name.Split(new char[] { '_' });
                    if (strs.Length == 2)
                    {
                        clipLenName = strs[0];
                    }
                    clipLenName += "_len";
                    if (paras.Find(val => { return val.name == clipLenName; }) == null)
                        paras.Add(new AnimatorControllerParameter { name = clipLenName, type = AnimatorControllerParameterType.Float, defaultFloat = v.clip1.length });
                }
                if (!v.isOne)
                {
                    string[] strs = v.name.Split(new char[] { '_' });
                    if (strs.Length != 2)
                    {
                        Debug.LogError("不能添加融合状态(名字不标准):" + v.name);
                    }
                    if (v.clip1 != null) //上半身动作层
                    {
                        if (!bodyUpStates.ContainsKey(strs[0]))
                        {
                            //添加攻击状态
                            var state = layers[1].stateMachine.AddState(strs[0], layers[1].stateMachine.entryPosition +
                                new Vector3(400 + 60 * Mathf.Sin(layers[1].stateMachine.states.Length * 20f), -400 + 80 * layers[1].stateMachine.states.Length, 0));
                            var trans = state.AddTransition(bodyUpLayerNullState);
                            trans.duration = 0;
                            trans.AddCondition(AnimatorConditionMode.If, 1, "stop_body_up");
                            state.motion = v.clip1;
                            //记录动画时长
                            string strTemp = strs[0] + "_len";
                            if (paras.Find(val => { return val.name == strTemp; }) == null)
                                paras.Add(new AnimatorControllerParameter { name = strTemp, type = AnimatorControllerParameterType.Float, defaultFloat = v.clip1.length });

                            bodyUpStates.Add(strs[0], state);
                        }
                    }
                    if (v.clip2 != null) //下半身动作层
                    {
                        if (!bodyDownStates.ContainsKey(strs[1]))
                        {
                            //添加攻击状态
                            var state = layers[2].stateMachine.AddState(strs[1], layers[2].stateMachine.entryPosition +
                                new Vector3(400 + 60 * Mathf.Sin(layers[2].stateMachine.states.Length * 20f), -400 + 80 * layers[2].stateMachine.states.Length, 0));
                            var trans = state.AddTransition(bodyDownLayerNullState);
                            trans.duration = 0;
                            trans.AddCondition(AnimatorConditionMode.If, 1, "stop_body_down");
                            state.motion = v.clip2;
                            bodyDownStates.Add(strs[1], state);
                        }
                    }
                }
            }
            animatorCtrAsset.parameters = paras.ToArray();
            string[] strsTemp = new string[]{"common_trans","body_up_trans","body_down_trans"};
            //添加所有动作转换
            for (int i = 0; i < 3; i++)
            {
                foreach (var cs in layers[i].stateMachine.states)
                {
                    if (cs.state.name == "null")
                        continue;
                    foreach (var othercs in layers[i].stateMachine.states)
                    {
                        if ( othercs.state.name == cs.state.name)
                            continue; 
                        var trans = othercs.state.AddTransition(cs.state);
                        trans.AddCondition(AnimatorConditionMode.Equals, Animator.StringToHash(cs.state.name), strsTemp[i]);
                        trans.hasFixedDuration = false;
                    }
                }
            }
             
            animatorCtrAsset.AddLayer("base layer");

            for (int i = 0; i < 3;i++ )
            {
                if (AssetDatabase.GetAssetPath(animatorCtrAsset) != "")
                    AssetDatabase.AddObjectToAsset(layers[i].stateMachine, AssetDatabase.GetAssetPath(animatorCtrAsset));

                foreach (var v in layers[i].stateMachine.states)
                {
                    AssetDatabase.AddObjectToAsset(v.state, AssetDatabase.GetAssetPath(animatorCtrAsset));
                    foreach (var v1 in v.state.transitions)
                    {
                        AssetDatabase.AddObjectToAsset(v1, AssetDatabase.GetAssetPath(animatorCtrAsset));
                    }
                }

                animatorCtrAsset.AddLayer(layers[i]);  
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets(); 
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); 
        } 
    }

    AnimatorController animatorCtrAsset = null;
    AvatarMask bodyUpMask;
    AvatarMask bodyDownMask;
    string error="";
    void OnGUI()
    {
        animatorCtrAsset = EditorGUILayout.ObjectField("animator ctr obj:", animatorCtrAsset, typeof(AnimatorController), false) as AnimatorController;
        bodyUpMask = EditorGUILayout.ObjectField("上半身骨骼mask:", bodyUpMask, typeof(AvatarMask), false) as AvatarMask;
        bodyDownMask = EditorGUILayout.ObjectField("下半身骨骼mask:", bodyDownMask, typeof(AvatarMask), false) as AvatarMask;

        if (animatorCtrAsset == null)
        {
            ShowNotification(new GUIContent("选择一个动画控制器"));
            return;
        }
        //if (bodyUpMask == null)
        //{
        //    ShowNotification(new GUIContent("选择上半身动画mask"));
        //    return;
        //}
        //if (bodyDownMask == null)
        //{
        //    ShowNotification(new GUIContent("选择下半身动画mask"));
        //    return;
        //}
        RemoveNotification();

        EditorGUILayout.BeginHorizontal(); 

        if (animatorCtrAsset != null)
        {
            ShowStateClipInfos();
        }

        EditorGUILayout.BeginVertical();

        ShowAniClipBaseArea();

        AutoSetStateClip();

        GenAniCtr();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        GUI.color = Color.red;
        GUILayout.Label(error, new GUILayoutOption[] { GUILayout.MinHeight(30) });
        GUI.color = Color.white;

    }
}
