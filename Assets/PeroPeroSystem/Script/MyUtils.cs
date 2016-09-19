using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class MyUtils : MonoBehaviour {
    public static GameObject cloneGameObject(GameObject baseGameObject, GameObject parentGameObject = null) {
        GameObject ret = new GameObject();
        ret.transform.position = baseGameObject.transform.position;
        ret.transform.rotation = baseGameObject.transform.rotation;
        if (parentGameObject == null) {
            ret.transform.parent = null;
        } else {
            ret.transform.parent = parentGameObject.transform;
        }
        return ret;
    }

    public static Transform cloneTransform(Vector3 pos, Quaternion rot, Transform parent = null) {
        Transform ret = new GameObject().transform;
        ret.position = pos;
        ret.rotation = rot;
        ret.parent = parent;
        return ret;
    }

    public static void deepCopyTransform(Transform to, Vector3 pos, Quaternion rot, Transform parent = null) {
        to.position = pos;
        to.rotation = rot;
        to.parent = parent;
    }

    public static Vector3 clamp(Vector3 vec, Vector3 max, Vector3 min) {
        vec.x = Mathf.Clamp(vec.x, min.x, max.x);
        vec.y = Mathf.Clamp(vec.y, min.y, max.y);
        vec.z = Mathf.Clamp(vec.z, min.z, max.z);
        return vec;
    }

    public static void LookAtExtended(Transform focusedTF, Transform targetTF) {
        // 左手系なのに注意！
        var upward = (focusedTF.position - targetTF.position).normalized;
        var right = Vector3.Cross(upward, targetTF.forward).normalized;
        //var right = Vector3.Cross(upward, focusedTF.forward).normalized;
        var forward = Vector3.Cross(right, upward).normalized;

        //var targetEuler = Quaternion.LookRotation(forward, upward).eulerAngles;

        focusedTF.rotation = Quaternion.Slerp(focusedTF.rotation,
                                              Quaternion.LookRotation(forward, upward),
                                              0.9f);
    }


    //public static void LookAtExtended(Transform focusedTF, Transform targetTF, Vector3 forward) {
    //    // 左手系なのに注意！
    //    Vector3 right = -(targetTF.position - focusedTF.position);
    //    Vector3 upwards = Vector3.Cross(targetTF.forward, right);
    //    //Vector3 upwards = Vector3.Cross(focusedTF.forward, right);
    //    forward = Vector3.Cross(right, upwards);
    //    Quaternion targetRot = Quaternion.Slerp(focusedTF.rotation,
    //                                            Quaternion.LookRotation(forward, upwards),
    //                                            0.3f);
    //    Vector3 diffrot = (Quaternion.Inverse(focusedTF.rotation) *  targetRot).eulerAngles;

    //    float angle = Quaternion.Angle(focusedTF.rotation, targetRot);

    //    if (Mathf.Abs(angle) > 120) {
    //        print(angle);
    //        return;
    //    }
    //    focusedTF.rotation *= Quaternion.Euler(diffrot);
    //}

    public static void LookAtExtended(Transform focusedTF, Transform targetTF, Vector3 forward) {
        // 左手系なのに注意！
        Vector3 right = -(targetTF.position - focusedTF.position);
        Vector3 upwards = Vector3.Cross(targetTF.forward, right);
        //Vector3 upwards = Vector3.Cross(focusedTF.forward, right);
        forward = Vector3.Cross(right, upwards);
        focusedTF.rotation = Quaternion.Slerp(focusedTF.rotation,
                                              Quaternion.LookRotation(forward, upwards),
                                              0.3f);
    }

    public static void LookAtExtended_old2(Transform focusedTF, Transform targetTF, Vector3 forward) {
        Quaternion lookrot = Quaternion.LookRotation(targetTF.position - focusedTF.position);
        focusedTF.rotation = lookrot * Quaternion.FromToRotation(forward, Vector3.forward);
    }

    public static void LookAtExtended_old(Transform focusedTF, Transform targetTF, Vector3 forward) {
        //Quaternion initRot = new Quaternion();
        //initRot.eulerAngles = Vector3.Cross(focusedTF.localEulerAngles, Vector3.forward);

        Vector3 initEuler = focusedTF.localEulerAngles;
        Quaternion lookrot = Quaternion.LookRotation(targetTF.position - focusedTF.position) * Quaternion.FromToRotation(forward, Vector3.forward);
        Vector3 euler = lookrot.eulerAngles;
        Quaternion input = Quaternion.Euler(0, euler.y, euler.z);
        focusedTF.rotation = input;

        focusedTF.localRotation = Quaternion.Euler(focusedTF.localEulerAngles.x,
                                                   focusedTF.localEulerAngles.y,
                                                   focusedTF.localEulerAngles.z);

        //focusedTF.rotation = lookrot;
        //focusedTF.localRotation = focusedTF.localRotation * Quaternion.Inverse(initRot);
        print(focusedTF.localEulerAngles);
        //        focusedTF.Rotate(-localRotation.x, -localRotation.y, -localRotation.z, Space.Self);
    }


    // Return 1 when focusedObject is root
    public static int getHierarchyDepth(GameObject focusedObject) {
        int hierarchyDepth = 1;
        Transform tracingTF = focusedObject.transform;
        for (; tracingTF.parent != null; hierarchyDepth++) {
            tracingTF = tracingTF.parent;
        }
        return hierarchyDepth;
    }

    public static void print(string str, UnityEngine.UI.Text textUI = null) {
        //if (textUI != null) {
        //    textUI.text = str;
        //}
    }

    public static void calcIK(GameObject target, List<GameObject> jointsFromChildToParent, List<float> linkLengthes) {
        var tempTargetTF = MyUtils.cloneTransform(target.transform.position, target.transform.rotation);
        for (int i = 0; i < jointsFromChildToParent.Count ; i++) {
            var joint = jointsFromChildToParent[i];
            var length = linkLengthes[i];
            MyUtils.LookAtExtended(joint.transform, tempTargetTF);
            tempTargetTF.Translate(-length * (tempTargetTF.position - joint.transform.position).normalized, Space.World);
            //tempTargetTF.rotation = joint.transform.rotation;
            //tempTargetTF.Translate(-length * jointFront, Space.Self);
        }
    }

    public static void syncState(Transform focused, Transform target) {
        focused.position = target.position;
        focused.rotation = target.rotation;
    }

    public static Vector3 getCenterPos(params Transform[] transforms) {
        var pos = Vector3.zero;
        foreach (Transform tf in transforms) {
            pos += tf.position;
        }
        return pos / transforms.Count();
    }

    public static string list2strings<T>(List<T> list, string separater = ",") {
        return String.Join(separater, list.Select(l => l.ToString()).ToArray());
    }
    public static string list2strings<T>(T[] list, string separater = ",") {
        return String.Join(separater, list.Select(l => l.ToString()).ToArray());
    }

    public static float calcVirtualRadius(params Vector3[] points) {
        return calcVirtualRadius(new List<Vector3>(points));
    }

    public static float calcVirtualRadius(List<Vector3> points) {
        if (points.Count == 0) {
            return 0;
        }
        var center = (points.Aggregate((prev, curr) => prev + curr)) / (float)points.Count();
        var radiuses = points.Select(point => Vector3.Distance(point, center));
        return ((radiuses.Aggregate((prev, curr) => prev + curr)) / (float)radiuses.Count());
    }

    public static bool IsCloseInDeg(float ang, float reference, float tolerance) {
        return Mathf.Abs((reference - ang) % 360) < tolerance;
    }

    public static string addZero(int i) {
        return (i < 10 ? "0" + i.ToString() : i.ToString());
    }

    public static Vector3 Abs(Vector3 vec) {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }


    public static float lowpassFilter(float newData, float prevData, float newDataRate = 0.1f) {
        return newDataRate * newData + (1.0f - newDataRate) * prevData;
    }

    public class LowpassFilter {
        bool isInitialized = false;
        float prevVal;
        float rate;
        public LowpassFilter(float rate = 0.1f) {
            this.rate = rate;
        }
        public float getFilteredVal(float newVal) {
            if (!isInitialized) {
                this.prevVal = newVal;
                isInitialized = true;
            }
            prevVal = rate * newVal + (1.0f - rate) * prevVal;
            return prevVal;
        }
    }

    public abstract class DataProcesser<T> {
        protected int num;
        public List<T> vals { get; private set; }
        public List<float> time { get; private set; }
        private float min_dt = 0.0001f;

        public DataProcesser(int numOfData = 10) {
            this.num = numOfData;
            this.vals = new List<T>(numOfData);
            this.time = new List<float>(numOfData);
        }

        public void AddData(T data) {
            var sizeDiff = this.vals.Count - this.num + 1;
            if (sizeDiff > 0) {
                this.remove(0, sizeDiff);
                this.add(data);
                return;
            } else if (this.vals.Count() < this.num) {
                for (int i = 0; i < this.num - 1; i++) {
                    this.add(data);
                }
                return;
            }
            this.add(data);
        }

        public T prev { get { return this.vals[this.vals.Count - 2]; } }

        public T curr { get { return this.vals[this.vals.Count - 1]; } }

        protected void add(T data) {
            this.vals.Add(data);
            this.time.Add(Time.time);
        }

        protected void remove(int index, int count) {
            this.vals.RemoveRange(index, count);
            this.time.RemoveRange(index, count);
        }

        public abstract T getDifferentiation();

        public abstract T getDifferentiationFivept();

    }
    public class Vector3DataProcesser : DataProcesser<Vector3> {
        public Vector3DataProcesser(int numOfData = 10) : base(numOfData) {
        }
        public override Vector3 getDifferentiation() {
            var count = this.vals.Count;
            if (count < 2) {
                return Vector3.zero;
            }
            return (this.vals[count - 1] - this.vals[count - 2]) / (this.time[count - 1] - this.time[count - 2]);
        }

        public override Vector3 getDifferentiationFivept() {
            var count = this.vals.Count;
            if (count < 4) {
                return this.getDifferentiation();
            }
            var dt = (this.time[count - 1] - this.time[count - 5]) / 4.0f;
            return (this.vals[count - 5] - 8 * this.vals[count - 4] + 8 * this.vals[count - 2] - this.vals[count - 1]) / 12 * dt;
        }
    }

    public class QuatanionDataProcesser : DataProcesser<Quaternion> {
        public QuatanionDataProcesser(int numOfData = 10) : base(numOfData) {
        }

        public override Quaternion getDifferentiation() {
            return Quaternion.identity;
        }
        public override Quaternion getDifferentiationFivept() {
            return Quaternion.identity;
        }
    }

    public class FloatDataProcesser : DataProcesser<float> {
        public override float getDifferentiation() {
            var count = this.vals.Count;
            if (count < 2) {
                return 0;
            }
            return (this.vals[count - 1] - this.vals[count - 2]) / (this.time[count - 1] - this.time[count - 2]);
        }

        public override float getDifferentiationFivept() {
            var count = this.vals.Count;
            if (count < 4) {
                return this.getDifferentiation();
            }
            var dt = (this.time[count - 1] - this.time[count - 5]) / 4.0f;
            return (this.vals[count - 5] - 8 * this.vals[count - 4] + 8 * this.vals[count - 2] - this.vals[count - 1]) / 12 * dt;
        }

    }




#if UNITY_EDITOR
    // http://qiita.com/seinosuke/items/e8c7ee2e1f98a76070e2
    public static void addTag(string tagname) {
        UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if ((asset != null) && (asset.Length > 0)) {
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; ++i) {
                if (tags.GetArrayElementAtIndex(i).stringValue == tagname) {
                    return;
                }
            }

            int index = tags.arraySize;
            tags.InsertArrayElementAtIndex(index);
            tags.GetArrayElementAtIndex(index).stringValue = tagname;
            so.ApplyModifiedProperties();
            so.Update();
        }
    }
#endif
}
