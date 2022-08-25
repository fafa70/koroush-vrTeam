Public Enum UsarRunStatus

    'before the run starts, preparation time
    Preparing = 1

    'preparation/configuration completed, ready to start
    Ready = 2

    'the actual run (20 mins)
    Running = 3

    'delivery time (10 mins)
    Reporting = 4

    'run finished
    Done = 5

End Enum
