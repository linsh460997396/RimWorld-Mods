using BepInEx;
using BepInEx.Unity.Mono;
using CellSpace;
using MetalMaxSystem.Unity;
using RimWorld;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows.WebCam;
using static UnityEngine.GraphicsBuffer;

namespace MCFramework
{
    [BepInPlugin("com.MCFramework.RimWorld", "MCFramework_20250823", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        public static bool firstQ;
        public GameObject odin;
        ExploreSlice exploreSlice;
        public Camera camera;

        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float rotationSensitivity = 100f;
        public float zoomSpeed = 10f;
        [Header("Rotation Constraints")]
        public float minVerticalAngle = -89f;
        public float maxVerticalAngle = 89f;

        //↓入口函数处运用示范↓
        private void Awake()
        {
            //环世界在菜单就创建的话,进游戏会被泰南销毁
        }

        private void Start()
        {
            //环世界在菜单就创建的话,进游戏会被泰南销毁
        }

        public void Init()
        {
            //读取AB包中资源
            AssetBundleLoader.Instance.LoadAllFromMemoryAsync<GameObject>("BepInEx/plugins/MCFramework/abtest");

            exploreSlice = new ExploreSlice();
            Debug.Log("挂载ExploreSlice模型爆炸功能！");

            CPEngine.Active();
            Debug.Log("激活MC框架");
            CPEngine.generateColliders = true;
        }

        void ToggleCamera(bool useSubCam)
        {
            camera = GameObject.Find("Camera").GetComponent<Camera>();
            if (camera != null)
            {
                if (CPEngine.horizontalMode)
                {
                    //2D横板模式用正交投影
                    camera.orthographic = true;
                    camera.orthographicSize = camera.orthographicSize;
                    Debug.Log("正交镜头:摄像机默认正交尺寸=" + camera.orthographicSize);
                }
                else if (CPEngine.singleLayerTerrainMode)
                {
                    //3D单层地形模式用正交投影
                    camera.orthographic = true;
                    camera.orthographicSize = camera.orthographicSize;
                    Debug.Log("正交镜头:摄像机默认正交尺寸=" + camera.orthographicSize);
                    //camera.gameObject.transform.rotation = Quaternion.Euler(90, 0, 0); //原横板模式设计的摄像机绕X轴顺时针转90度以俯视X-Z平面
                }
                else
                {
                    //正常3D模式的镜头应另行支持鼠标旋转屏
                    //camera.gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
                    //camera.orthographic = false;
                    //Debug.Log("透视镜头:摄像机默认视野大小=" + camera.fieldOfView);

                    Debug.Log("禁用环世界原来的相机组件");
                    camera.enabled = false;

                    CPEngine.Instance.gameObject.transform.position = camera.transform.position;
                    //CPEngine.Instance.gameObject.transform.rotation = camera.transform.rotation;
                    Vector3 cp = CPEngine.Instance.gameObject.transform.position;

                    //Debug.Log("在CPEngine物体下方2m处放一个土球");
                    //CellInfo newInfo = CPEngine.PositionToCellInfo(new Vector3(cp.x, cp.y - 2, cp.z));
                    //Cell.PlaceBlock(newInfo, 8);

                    Debug.Log("在CPEngine物体上创建FirstPersonController组件");
                    if (CPEngine.Instance.gameObject.GetComponent<FirstPersonController>() == null)
                        CPEngine.Instance.gameObject.AddComponent<FirstPersonController>();


                }
                //camera.gameObject.transform.position = new Vector3(0, 20, 0);
            }
            else
            {
                Debug.LogError("没有找到主摄像机！");
            }
            //Cursor.lockState = CursorLockMode.Locked; // 第一人称控制
        }

        void HandleHeightAdjustment()
        {
            // H键上升
            if (Input.GetKey(KeyCode.H))
            {
                float direction = 1f;

                // Shift+H组合键下降
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    direction = -1f;
                }

                CPEngine.Instance.gameObject.transform.position += Vector3.up * direction * moveSpeed * Time.deltaTime;
            }
        }

        void HandleRotationControls()
        {
            // Q/E绕Z轴旋转
            if (Input.GetKey(KeyCode.Q))
            {
                CPEngine.Instance.gameObject.transform.Rotate(0, 0, rotationSensitivity * Time.deltaTime, Space.Self);
            }
            if (Input.GetKey(KeyCode.E))
            {
                CPEngine.Instance.gameObject.transform.Rotate(0, 0, -rotationSensitivity * Time.deltaTime, Space.Self);
            }

            // R/T绕X轴旋转
            if (Input.GetKey(KeyCode.R))
            {
                CPEngine.Instance.gameObject.transform.Rotate(rotationSensitivity * Time.deltaTime, 0, 0, Space.Self);
            }
            if (Input.GetKey(KeyCode.T))
            {
                CPEngine.Instance.gameObject.transform.Rotate(-rotationSensitivity * Time.deltaTime, 0, 0, Space.Self);
            }

            // Y/U绕Y轴旋转
            if (Input.GetKey(KeyCode.Y))
            {
                CPEngine.Instance.gameObject.transform.Rotate(0, rotationSensitivity * Time.deltaTime, 0, Space.Self);
            }
            if (Input.GetKey(KeyCode.U))
            {
                CPEngine.Instance.gameObject.transform.Rotate(0, -rotationSensitivity * Time.deltaTime, 0, Space.Self);
            }
        }

        private void Update()
        {
            #region 外部模型导入

            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                //打印物体列表
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    Debug.Log("GameObject: " + obj.name + " " + obj.transform.position.ToString());
                    //obj.SetActive(false);
                }
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                GameObject.Find("GravshipMask").SetActive(false);
                GameObject.Find("ReverZoneDummy").SetActive(false);
                GameObject.Find("WaterDepth").SetActive(false);
            }

            if (AssetBundleLoader.currentObjectGroup != null && Input.GetKeyDown(KeyCode.PageDown))
            {
                Debug.Log("AssetBundleLoader.currentObjectGroup.Length => " + AssetBundleLoader.currentObjectGroup.Length.ToString());
                for (int i = 0; i < AssetBundleLoader.currentObjectGroup.Length; i++)
                {
                    Debug.Log("查询AB包中第" + i.ToString() + "个元素成功！");
                    Debug.Log("GameObject " + i + " Name: " + AssetBundleLoader.currentObjectGroup[i].name);
                }

                if (GameObject.Find("Camera") != null)
                {
                    //gameObjectGroup[0]是奥丁，gameObjectGroup[1]是跳虫，目前AB包（abtest）内这只有2个预制体。
                    odin = Instantiate(AssetBundleLoader.currentObjectGroup[0] as GameObject);
                    odin.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    Debug.Log("创建奥丁！");
                }

                //检查预制体模型的动画信息
                Animation[] animations = odin.GetComponents<Animation>();
                foreach (Animation animation in animations)
                {
                    Debug.Log("odinAnimation Name: " + animation.name);
                    Debug.Log("odinCurrentAnimationClip Name: " + animation.clip.name);
                }

                //删除预制体内的刚体，防止子物体参与物理引擎，让子模型完全按主体的Transform行动
                //Rigidbody odinRigidbody = odin.GetComponent<Rigidbody>();
                //if (odinRigidbody != null)
                //{
                //    Destroy(odinRigidbody);
                //}

                #region 对游戏物体进行镭射检测并输出碰撞到的物体名

                //Debug.Log("对周围10.0半径内的游戏对象进行镭射检测...");
                //LayerMask layerMask = ~0;
                //Collider[] hits = Physics.OverlapSphere(CPEngine.Instance.gameObject.transform.position, 100.0f, layerMask);
                ////输出检测到的游戏对象的名称
                //foreach (Collider hit in hits)
                //{
                //    //得到游戏对象
                //    GameObject hitObject = hit.gameObject;
                //    Debug.Log("检测到游戏对象: " + hitObject.name);
                //}

                #endregion

                #region 衔接

                //衔接前游戏物体的世界坐标系的旋转和位置与要衔接的主体保持一致
                odin.transform.position = CPEngine.Instance.gameObject.transform.position;
                odin.transform.rotation = CPEngine.Instance.gameObject.transform.rotation;

                //将odin设置为子对象
                //odin.transform.parent = CPEngine.Instance.gameObject.transform;
                Debug.Log("奥丁已拼接到CPEngine");

                #endregion

            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                //切换摄像机
                ToggleCamera(true);
            }

            HandleHeightAdjustment();
            HandleRotationControls();

            if (Input.GetKeyDown(KeyCode.Q) && firstQ != true)
            {
                firstQ = true;
                Debug.Log("按下了Q键");

                Init(); //读取素材并初始化框架
            }
            else if (Input.GetKeyUp(KeyCode.Q) && firstQ == true)
            {
                CPEngine.Tick();
                Debug.Log("CPEngine.Tick()");
            }

            #endregion

            #region 行走动画

            if (odin != null && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
            {
                odin.GetComponent<Animation>().Play("Walk");
            }

            #endregion

            #region 爆破功能

            //if (odin != null && Input.GetMouseButtonDown(0))
            //{
            //    // 获取鼠标点击的屏幕坐标
            //    Vector3 mousePos = Input.mousePosition;
            //    Vector3 rayOrigin = Camera.main.transform.position; // 射线的起点：摄像机位置
            //    Vector3 rayDirection = Camera.main.ScreenPointToRay(mousePos).direction; // 射线的方向：从摄像机到鼠标点击

            //    // 投射射线
            //    Ray ray = new Ray(rayOrigin, rayDirection);
            //    RaycastHit hit;
            //    float rayLength = 10000f; // 射线的长度，可以根据需要调整
            //    //LayerMask layerMask = LayerMask.GetMask("Default"); // 射线投射的目标层
            //    LayerMask layerMask = ~0;

            //    if (Physics.Raycast(ray, out hit, rayLength, layerMask))
            //    {
            //        // 射线击中了物体
            //        Debug.Log("Hit object: " + hit.transform.name);
            //        if (hit.transform.gameObject.GetComponent<DrawBounds>() == null)
            //        {
            //            hit.transform.gameObject.AddComponent<DrawBounds>();
            //            Debug.Log("挂载爆破功能: " + hit.transform.name);
            //        }


            //        exploreSlice.Explore(hit.transform.gameObject, ray.direction);
            //    }
            //    else
            //    {
            //        // 射线没有击中任何物体
            //        Debug.Log("No object hit");
            //        // 使用Debug.DrawRay在Scene视图中绘制射线
            //        Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.red, 0.5f); // 红色射线，持续0.5秒
            //    }
            //}

            #endregion
        }
    }
}
