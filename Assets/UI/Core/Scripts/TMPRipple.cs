using TMPro;
using UnityEngine;

namespace ContentAlchemy
{
    public class TMPRippleCharacter
    {
        public float[] Value { get; set; } = new float[4];
        public bool[] GoingUp { get; set; } = new bool[4];
        public float Speed { get; set; }
    }

    public class TMPRipple : MonoBehaviour
    {
        [SerializeField]
        private bool _startImmediately;
        public bool StartImmediately => _startImmediately;
        [SerializeField]
        private TMP_Text _textMesh;
        public TMP_Text TextMesh => _textMesh;
        [SerializeField]
        private Color _darkColor = Color.blue;
        public Color DarkColor => _darkColor;
        [SerializeField]
        private Color _lightColor = Color.cyan;
        public Color LightColor => _lightColor;
        [SerializeField]
        private float _minSpeed = 1f;
        public float MinSpeed => _minSpeed;
        [SerializeField]
        private float _maxSpeed = 1f;
        public float MaxSpeed => _maxSpeed;
        [SerializeField]
        private int _modulus = 10;
        public int Modulus => _modulus;

        private TMPRippleCharacter[] charData;
        private bool reinit = false;
        private bool rippling = false;
        private int charCount = 0;

        private void Start()
        {
            if (StartImmediately)
            {
                reinit = true;
            }
            charCount = TextMesh.textInfo.characterCount;
        }

        public void Reinit()
        {
            charCount = TextMesh.textInfo.characterCount;
            if (charCount <= 0)
            {
                return;
            }
            charData = new TMPRippleCharacter[charCount];
            for (var iChar = charCount - 1; iChar >= 0; iChar--)
            {
                // Set up the random directions.
                TMPRippleCharacter data = new TMPRippleCharacter();
                data.Speed = Random.Range(MinSpeed, MaxSpeed);
                for (var iVert = 0; iVert < 4; iVert++)
                {
                    data.GoingUp[iVert] = Random.value > 0.5f;
                    data.Value[iVert] = Random.value;
                }
                charData[iChar] = data;

                // Set up the initial random values
                var materialIndex = TextMesh.textInfo.characterInfo[iChar].materialReferenceIndex;//get characters material index
                Color32[] vertexColors = TextMesh.textInfo.meshInfo[materialIndex].colors32;
                var vertexIndex = TextMesh.textInfo.characterInfo[iChar].vertexIndex;//get its vertex index
                for (var iVert = 0; iVert < 4; iVert++)
                {
                    vertexColors[vertexIndex + 0] = DarkColor + (LightColor - DarkColor) * data.Value[iVert];
                }
            }
            TextMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            reinit = false;
            rippling = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (reinit)
            {
                Reinit();
            }
            if (rippling)
            {
                var count = TextMesh.textInfo.characterCount;
                if (count != charCount)
                {
                    Reinit();
                }
                if (count > 0)
                {
                    for (var iChar = count - 1; iChar >= 0; iChar--)
                    {
                        var materialIndex = TextMesh.textInfo.characterInfo[iChar].materialReferenceIndex;//get characters material index
                        Color32[] vertexColors = TextMesh.textInfo.meshInfo[materialIndex].colors32;
                        var vertexIndex = TextMesh.textInfo.characterInfo[iChar].vertexIndex;//get its vertex index
                        TMPRippleCharacter data = charData[iChar];
                        for (var iVert = 0; iVert < 4; iVert++)
                        {
                            if (data.GoingUp[iVert])
                            {
                                data.Value[iVert] += Time.deltaTime;
                                if (data.Value[iVert] > 1)
                                {
                                    data.Value[iVert] = 1;
                                    data.GoingUp[iVert] = false;
                                }
                            }
                            else
                            {
                                data.Value[iVert] -= Time.deltaTime;
                                if (data.Value[iVert] < 0)
                                {
                                    data.Value[iVert] = 0;
                                    data.GoingUp[iVert] = true;
                                }
                            }
                            vertexColors[vertexIndex + iVert] = DarkColor + (LightColor - DarkColor) * data.Value[iVert];
                        }
                    }
                    TextMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                }
            }
        }
    }
}