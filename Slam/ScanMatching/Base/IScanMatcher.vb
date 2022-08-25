Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

Public Interface IScanMatcher

    Sub ApplyConfig(ByVal config As Config)

    Function Match(ByVal manifold As Manifold, ByVal patch1 As Patch, ByVal patch2 As Patch, ByVal seed As Pose2D) As MatchResult

End Interface
