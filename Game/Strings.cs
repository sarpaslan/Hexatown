
//TEMP solution for now
public static class Strings
{
    public static string ToString(PlaceError error, Place place)
    {
        var sterror = "<color=red>";
        switch (error)
        {
            case PlaceError.NONE:
                return string.Empty;
            case PlaceError.NOT_ENOUGH_WORKER:
                var workPlace = place as WorkPlace;
                sterror += $"Not enough {workPlace.Tier.ToReadableString()}'s to work here.";
                if (workPlace.Tier == 0)
                {
                    sterror += " Build nearby houses";
                }
                else
                {
                    sterror += $"Upgrade nearby houses to {workPlace.Tier.ToReadableString()}'s house";
                }
                break;
            case PlaceError.NOT_ENOUGH_RESOURCES:
                var production = place as ProductionPlace;
                sterror += $"Not enough {production.Requires} you need {production.RequireAmount}";
                break;
            case PlaceError.NOT_ENOUGH_TAX:
                sterror += $"You don't have enough money (${place.Tax}) to maintain this place.";
                break;
            case PlaceError.SELECTION_NEEDED:
                var selection = place as SelectionWorkPlace;
                if (selection is Ranch ranch)
                {
                    if (ranch.SelectionCount == 0)
                        sterror += "You have to find animals first by exploring";
                    else
                        sterror += $"Select an animal type to grow here.";
                }
                else if (selection is FarmLand farm)
                {
                    if (farm.SelectionCount == 0)
                        sterror += "You have to find seeds first by exploring";
                    else
                        sterror += $"Select a crop type to grow here.";
                }
                break;
            case PlaceError.CANT_WORK_ON_THIS_SEASON:
                sterror += "Can't work on this season.";
                break;
        }
        return sterror + "</color>\n";
    }
}
