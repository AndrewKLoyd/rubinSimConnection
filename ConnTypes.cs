namespace Connection.ConnTypes
{
    //TODO: Modify when new sub was added
    public enum Sub
    {
        RegulatorComplex,
        Custom,
        SimInit,
        Mission,
        GroupTrajectory,
        MathModelSwitch,
        CustomSGRU,
        CustomSGRUEvent,
        CustomTab
    }

    //TODO: Modify when new pub was added
    public enum Pub
    {
        ANPA,
        ANPAGroup,
        ANPAGroupTab,
        SimEvent,
        CustomSim,
        SimBoundingBoxes,
        IsInKTS

    }
}