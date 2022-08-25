Imports UvARescue.Math

Public Class MatchResult

    Private _RawOdometryPose As Pose2D 'original odometry from sensor / log file)
    Private _EstimatedOdometryPose As Pose2D 'the scanmatcher's odometry estimate
    Private _Covariance As Matrix3 'matching covariance
    Private _Distance As Double 'matching distance

    Private _NumIterations As Integer
    Private _NumCorrespondencies As Integer
    Private _NumMilliseconds As Integer

    Private _Converged As Boolean

    Public Sub New(ByVal rawOdometryPose As Pose2D, ByVal estimatedOdometryPose As Pose2D, ByVal covariance As Matrix3, ByVal distance As Double, ByVal numIterations As Integer, ByVal numCorrespondencies As Integer, ByVal numMilliseconds As Integer, ByVal converged As Boolean)
        Me._RawOdometryPose = rawOdometryPose
        Me._EstimatedOdometryPose = EstimatedOdometryPose
        Me._Covariance = covariance
        Me._Distance = distance
        Me._NumIterations = numIterations
        Me._NumCorrespondencies = numCorrespondencies
        Me._NumMilliseconds = numMilliseconds
        Me._Converged = converged
    End Sub


    Public ReadOnly Property RawOdometryPose() As Pose2D
        Get
            Return Me._RawOdometryPose
        End Get
    End Property

    Public ReadOnly Property EstimatedOdometryPose() As Pose2D
        Get
            Return Me._EstimatedOdometryPose
        End Get
    End Property

    Public ReadOnly Property Covariance() As Matrix3
        Get
            Return Me._Covariance
        End Get
    End Property

    Public ReadOnly Property Distance() As Double
        Get
            Return Me._Distance
        End Get
    End Property

    Public ReadOnly Property NumIterations() As Integer
        Get
            Return Me._NumIterations
        End Get
    End Property

    Public ReadOnly Property NumCorrespondencies() As Integer
        Get
            Return Me._NumCorrespondencies
        End Get
    End Property

    Public ReadOnly Property NumMilliseconds() As Integer
        Get
            Return Me._NumMilliseconds
        End Get
    End Property

    Public ReadOnly Property Converged() As Boolean
        Get
            Return Me._Converged
        End Get
    End Property

End Class
