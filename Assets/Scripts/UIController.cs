using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public AffineTransformer affineTransformer;

    // input matriks transformasi
    public InputField Mat_A;
    public InputField Mat_B;
    public InputField Mat_C;
    public InputField Mat_D;

    [SerializeField] InputField DegreeInput;
    [SerializeField] Button TransformButton;

    // Start is called before the first frame update
    void Start()
    {
        TransformButton.onClick.AddListener(TransformButton_OnClick);
        DegreeInput.onValueChanged.AddListener(DegreeInput_OnEndEdit);
    }

    public void TransformButton_OnClick()
    {
        affineTransformer.ExecuteAffineTransformation(float.Parse(Mat_A.text), float.Parse(Mat_B.text), float.Parse(Mat_C.text), float.Parse(Mat_D.text));
    }

    void DegreeInput_OnEndEdit(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            float degree = float.Parse(value);

            float a = Mathf.Cos(degree * Mathf.Deg2Rad);
            float b = Mathf.Sin(degree * Mathf.Deg2Rad);
            float c = -Mathf.Sin(degree * Mathf.Deg2Rad);
            float d = Mathf.Cos(degree * Mathf.Deg2Rad);

            Mat_A.text = a.ToString();
            Mat_B.text = b.ToString();
            Mat_C.text = c.ToString();
            Mat_D.text = d.ToString();
        }
    }
}
