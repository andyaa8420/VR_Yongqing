using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class Util
{

    public static string CovertCamelCaseToSentence(string input) {
        string output = Regex.Replace(input, @"\p{Lu}", m => " " + m.Value.ToLower());
        output = output.Trim();
        output = char.ToUpper(output[0]) + output.Substring(1);

        return output;
    }

    public static void MoveX(GameObject gameObject, float distance, bool isResize)
    {
        var rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 size = new Vector2();
            if (isResize)
            {
                size.x = rectTransform.sizeDelta.x - Mathf.Abs(distance);
            }
            else
            {
                size.x = rectTransform.sizeDelta.x;
            }
            size.y = rectTransform.sizeDelta.y;
            rectTransform.sizeDelta = size;

            Vector3 position = new Vector3();
            if (isResize)
            {
                position.x = rectTransform.position.x + distance / 2 * gameObject.transform.lossyScale.x;
            }
            else
            {
                position.x = rectTransform.position.x + distance * gameObject.transform.lossyScale.x;
            }
            position.y = rectTransform.position.y;
            position.z = rectTransform.position.z;
            rectTransform.position = position;
        }
    }

    public static void MoveY(GameObject gameObject, float distance, bool isResize)
    {
        var rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 size = new Vector2();
            size.x = rectTransform.sizeDelta.x;
            if (isResize)
            {
                size.y = rectTransform.sizeDelta.y - Mathf.Abs(distance);
            }
            else
            {
                size.y = rectTransform.sizeDelta.y;
            }
            rectTransform.sizeDelta = size;

            Vector3 position = new Vector3();
            position.x = rectTransform.position.x;

            if (isResize)
            {
                position.y = rectTransform.position.y + distance / 2 * gameObject.transform.lossyScale.y;
            }
            else
            {
                position.y = rectTransform.position.y + distance * gameObject.transform.lossyScale.y;
            }
            position.z = rectTransform.position.z;
            rectTransform.position = position;
        }
    }
}
