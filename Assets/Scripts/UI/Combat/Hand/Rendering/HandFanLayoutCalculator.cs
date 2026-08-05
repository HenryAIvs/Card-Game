using UnityEngine;

namespace UI.Combat
{
    public static class HandFanLayoutCalculator
    {
        public struct LayoutPose
        {
            public Vector2 Position;
            public Quaternion Rotation;
            public Vector3 Scale;

            public LayoutPose(Vector2 position, Quaternion rotation, Vector3 scale)
            {
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }
        }

        public static LayoutPose BuildPose(
            int index,
            int count,
            int hoveredIndex,
            int aimingIndex,
            int focusIndex,
            float cardSpacing,
            float maxRotation,
            float curveHeight,
            float hoverLift,
            float hoverScale,
            float aimingLift,
            float aimingScale,
            float neighbourPush
        )
        {
            float t = count == 1 ? 0.5f : (float)index / (count - 1);
            float centered = (t * 2f) - 1f;

            float x = centered * cardSpacing * Mathf.Max(0, count - 1) * 0.5f;
            float y = (1f - (centered * centered)) * curveHeight;
            float zRotation = -centered * maxRotation;

            if (focusIndex >= 0)
            {
                if (index < focusIndex)
                    x -= neighbourPush;
                else if (index > focusIndex)
                    x += neighbourPush;
            }

            Vector3 targetScale = Vector3.one;

            if (index == hoveredIndex)
            {
                y += hoverLift;
                targetScale = Vector3.one * hoverScale;
            }

            if (index == aimingIndex)
            {
                y += aimingLift;
                targetScale = Vector3.one * aimingScale;
            }

            return new LayoutPose(
                new Vector2(x, y),
                Quaternion.Euler(0f, 0f, zRotation),
                targetScale
            );
        }
    }
}