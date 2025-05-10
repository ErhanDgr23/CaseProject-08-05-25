using _project.Interface;
using _project.Settings;
using _project.Enums;
using UnityEngine;

namespace _project.Car
{
    public class CarColorChanger : MonoBehaviour
    {
        public ColorEnum MyColor;

        void OnEnable()
        {
            SettingSO settingSO = SettingSO.Instance;

            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);

                MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
                IColorChanger ChangeColor = child.GetComponent<IColorChanger>();

                if (meshRenderer == null)
                    continue;

                Material[] mats = meshRenderer.materials;
                int colorIndex = 0;

                for (int j = 0; j < mats.Length; j++)
                {
                    if (j == 0)
                    {
                        colorIndex = (int)MyColor - 1;

                        if (colorIndex >= 0 && colorIndex < settingSO.ColorMats.Length)
                            mats[j] = settingSO.ColorMats[colorIndex];
                    }
                }

                if (ChangeColor != null)
                    ChangeColor.ColorChanged(MyColor);

                meshRenderer.materials = mats;
            }

            Destroy(this);
        }
    }
}
