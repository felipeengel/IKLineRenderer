using UnityEngine;

public class LineRendererFollowPos : MonoBehaviour
{
    public class LineSegment
    {
        private Vector2 m_StartPoint;
        public Vector2 StartPoint => m_StartPoint;
        private Vector2 m_EndPoint;
        public Vector2 EndPoint => m_EndPoint;
        private readonly float m_SegmentLength;
        private float m_Angle = 1.6f;

        public LineSegment(Vector2 _start, float _segmentLength)
        {
            m_StartPoint = _start;
            m_SegmentLength = _segmentLength;
            CalculateEndPoint();
        }
        
        public LineSegment(LineSegment _parent, float _segmentLength)
        {
            m_StartPoint = _parent.m_EndPoint;
            m_SegmentLength = _segmentLength;
            CalculateEndPoint();
        }

        public void SetStartPoint(Vector2 _startPoint)
        {
            m_StartPoint = _startPoint;
            CalculateEndPoint();
        }

        private void CalculateEndPoint()
        {
            float dx = m_SegmentLength * Mathf.Cos(m_Angle);
            float dy = m_SegmentLength * Mathf.Sin(m_Angle);
            m_EndPoint = new Vector3(m_StartPoint.x+dx, m_StartPoint.y+dy);
        }
        
        public void Follow(Vector2 _target) 
        {
            Vector2 dir = _target - m_StartPoint;
            m_Angle = Mathf.Atan2(dir.y, dir.x);
            dir.Normalize();
            dir *= m_SegmentLength;
            dir *= -1;
            m_StartPoint = _target + dir;
            CalculateEndPoint();
        }
        
        public void Follow(LineSegment _child) 
        {
            Follow(_child.m_StartPoint);
        }

    }
    private LineSegment[] m_Segments;
    private Camera m_MainCamera;
    private Vector2 m_LineBaseWorldSpace = Vector2.zero;
    private Vector3 m_LineBaseScreenSpace;
    private Vector3 m_LastTouchPos;

    public LineRenderer lineRenderer;
    public bool attachToBase = false;
    public int maxSplinePoints = 50; //X points, X - 1 segments
    public float splinePointsSeparation = 0.2f;
    public Vector2 baseOffset;

    void Start()
    {
        m_MainCamera = Camera.main;
        m_LineBaseScreenSpace = new Vector3((Screen.width / 2f), 0f, 10f);
        m_LineBaseWorldSpace = m_MainCamera.ScreenToWorldPoint( m_LineBaseScreenSpace );
        m_LineBaseWorldSpace += baseOffset;
        m_Segments = new LineSegment[maxSplinePoints];
        m_Segments[0] = new LineSegment(m_LineBaseWorldSpace, splinePointsSeparation);
        for (int i = 1; i < m_Segments.Length; i++)
        {
            m_Segments[i] = new LineSegment(m_Segments[i-1], splinePointsSeparation);
        }

        CreateSplinePoints();
    }
    
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var touchPos = m_MainCamera.ScreenToWorldPoint(Input.mousePosition);
            touchPos.z = 10f;
            if ((touchPos - m_LastTouchPos).magnitude > 0.1f)
            {
                UpdateSegments(touchPos);
                m_LastTouchPos = touchPos;
            }
        }
    }
    
    private void CreateSplinePoints()
    {
        lineRenderer.startWidth = 0.20f;
        lineRenderer.endWidth = 0.10f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.numCornerVertices = 5;
        lineRenderer.numCapVertices = 5;
        lineRenderer.positionCount = m_Segments.Length;
        UpdateLine();
    }

    private void UpdateSegments(Vector2 mousePos)
    {
        int total = m_Segments.Length;
        LineSegment end = m_Segments[total - 1];
        end.Follow(mousePos);

        for (int i = total - 2; i >= 0; i--)
        {
            m_Segments[i].Follow(m_Segments[i+1]);
        }

        if (attachToBase)
        {
            m_LineBaseWorldSpace = m_MainCamera.ScreenToWorldPoint( m_LineBaseScreenSpace );
            m_Segments[0].SetStartPoint(m_LineBaseWorldSpace);
            for (int i = 1; i < total; i++)
            {
                m_Segments[i].SetStartPoint(m_Segments[i - 1].EndPoint);
            }
        }
        UpdateLine();
    }

    private void UpdateLine()
    {
        for (int i = 0; i < m_Segments.Length; i++)
        {
            lineRenderer.SetPosition(i, m_Segments[i].StartPoint);
        }
    }
}
