using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BeamRenderUtils
{
    //line
    public static float StartOffset = 0.06f;
    public static float EndOffset = 2.5f;
    //dot
    public static float DotMinRadius = 0.036f;
    public static float DotMaxRadius = 0.2f;
    public static float scaleUnitMinThreshold = 2f;
    public static float scaleUnitMaxThreshold = 10f;


    public static void UpdateLine(LineRenderer line, Vector3 start, Vector3 end)
    {
        var direction = end - start;
        //Calculate start and end offset
        var lineStart = start + StartOffset * 0.5f * direction;
        var lineEnd = end - EndOffset * 0.5f * direction;

        //Update LineRender positions
        line.SetPosition(0, lineStart);
        var actualBeamDis = Vector3.Distance(lineStart, lineEnd);
        var intervalDis = actualBeamDis / (line.positionCount - 1);

        for (int i = 1; i < line.positionCount; i++)
            line.SetPosition(i, lineStart + i * intervalDis * direction.normalized);
    }

    public static void UpdateHitDot(Transform dot, Vector3 hitPos, Vector3 headPos)
    {
        dot.position = hitPos;
        var hitDis = Vector3.Distance(headPos, hitPos);
        float radius = 0;
        var range = scaleUnitMaxThreshold - scaleUnitMinThreshold;
        var gradient = range > 0 ? (DotMaxRadius - DotMinRadius) / range : 0;
        if (hitDis > scaleUnitMaxThreshold)
            radius = DotMaxRadius;
        else if (hitDis < scaleUnitMinThreshold)
            radius = DotMinRadius;
        else
            radius = DotMinRadius + Mathf.Max(hitDis - scaleUnitMinThreshold, 0) * gradient;
        dot.localScale = radius * Vector3.one;
        dot.LookAt(headPos);
    }
}
