using System.Dynamic;
using UnityEngine;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Text;

public class LSystemsCrystal : MonoBehaviour
{

    [Header("Seed")]
    [Range(0, 10000)] public int seed;

    [Header("RNG Parameters")]
    [Range(0.5f, 50f)] public float lengthRNG;
    [Range(5.7f, 50f)] public float angleRNG;

    public Material crystalMaterial;

    private struct TurtleState
    {
        public Vector3 position;
        public Quaternion rotation;
        public float scale;
        public float thickness;
    }

    public class Rule
    {
        public char symbol;
        public string replacement;
    }

    public string axiom = "X";

    public List<Rule> rules = new List<Rule>
{
    new Rule
    {
        symbol = 'X',
        replacement = "F[+FX][-FX][&FX][^FX][/FX][\\FX]"
    },

    new Rule
    {
        symbol = 'F',
        replacement = "FF"
    }
};

    [Range(0, 6)] public int iterations = 2;
    public float angle = 30f;
    public float segmentLength = 1.5f;
    public float branchThickness = 0.6f;
    public float shrink = 0.85f;
    private string generatedString = "";
    private System.Random rng;


    void Start()
    {
        Generate();
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            Generate();
        }
        // Generate a new seed.
        if (Input.GetKeyDown(KeyCode.N))
        {
            seed = Random.Range(0, 10001);
            Generate();
        }
    }

    void Generate()
    {
        ClearPrevLines();

        // Initializes deterministic random generation using the seed
        rng = new System.Random(seed);

        generatedString = BuildString();
        DrawTurtle(generatedString);
        UpdateCollider();
    }

    string BuildString()
    {
        string current = axiom;
        for (int i = 0; i < iterations; i++)
        {
            StringBuilder next = new StringBuilder();

            foreach (char c in current)
            {
                string replacement = GetReplacement(c);
                next.Append(replacement);
            }

            current = next.ToString();
        }
        return current;
    }

    string GetReplacement(char c)
    {
        foreach (Rule rule in rules)
        {
            if (rule.symbol == c)
            {
                return rule.replacement;
            }
        }

        return c.ToString();
    }

    void DrawTurtle(string instructions)
    {
        Stack<TurtleState> stateStack = new Stack<TurtleState>();
        Vector3 currentPosition = Vector3.zero;
        Quaternion currentRotation = Quaternion.identity;
        float currentLength = segmentLength;
        float currentThickness = branchThickness;
        int index = 0;

        foreach (char c in instructions)
        {
            switch (c)
            {
                case 'F':
                    // Randomizes segment length consistently with the seed
                    currentLength = 1f + (float)rng.NextDouble() * lengthRNG;
                    Vector3 newPosition = currentPosition + currentRotation * Vector3.up * currentLength;
                    CreateLineSegment(currentPosition, newPosition, index, currentThickness);
                    index++;
                    currentPosition = newPosition;
                    currentThickness *= shrink;
                    break;

                case '+':
                    // Randomizes the rotation angle consistently with the seed
                    angle = 20f + (float)rng.NextDouble() * angleRNG;
                    currentRotation *= Quaternion.Euler(0, 0, angle);
                    break;

                case '-':
                    // Randomizes the rotation angle consistently with the seed
                    angle = 20f + (float)rng.NextDouble() * angleRNG;
                    currentRotation *= Quaternion.Euler(0, 0, -angle);
                    break;

                case '&':
                    // Rotates the branch positively around the X axis
                    angle = 20f + (float)rng.NextDouble() * angleRNG;
                    currentRotation *= Quaternion.Euler(angle, 0, 0);
                    break;

                case '^':
                    // Rotates the branch negatively around the X axis
                    angle = 20f + (float)rng.NextDouble() * angleRNG;
                    currentRotation *= Quaternion.Euler(-angle, 0, 0);
                    break;

                case '/':
                    // Rotates the branch positively around the Y axis
                    angle = 20f + (float)rng.NextDouble() * angleRNG;
                    currentRotation *= Quaternion.Euler(0, angle, 0);
                    break;

                case '\\':
                    // Rotates the branch negatively around the Y axis
                    angle = 20f + (float)rng.NextDouble() * angleRNG;
                    currentRotation *= Quaternion.Euler(0, -angle, 0);
                    break;

                case '[':
                    stateStack.Push(new TurtleState { position = currentPosition, rotation = currentRotation, scale = currentLength,  thickness = currentThickness });
                    break;

                case ']':
                    if (stateStack.Count > 0)
                    {
                        TurtleState state = stateStack.Pop();
                        currentPosition = state.position;
                        currentRotation = state.rotation;
                        currentLength = state.scale;
                        currentThickness = state.thickness;
                    }
                    break;
            }
        }
    }

    void CreateLineSegment(Vector3 init, Vector3 final, int index, float thickness)
    {
        // Creates a solid cylindrical mesh for each branch segment
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        segment.name = $"Segment_{index}";
        segment.transform.parent = transform;

        Renderer renderer = segment.GetComponent<Renderer>();
        renderer.material = crystalMaterial;

        Vector3 direction = final - init;
        float length = direction.magnitude;

        segment.transform.localPosition = (init + final) / 2f;

        // Aligns the cylinder with the direction of the branch
        segment.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);

        // Scales the cylinder to match the branch length and thickness
        segment.transform.localScale = new Vector3(thickness, length / 2f, thickness);
    }

    void UpdateCollider()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        col.center = transform.InverseTransformPoint(bounds.center);
        col.radius = bounds.extents.magnitude;
    }

    void ClearPrevLines()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}