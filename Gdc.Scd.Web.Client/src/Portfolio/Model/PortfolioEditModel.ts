export class PortfolioEditModel {
    public countryId: string;

    public wgs: string[];

    public availabilities: string[];

    public durations: string[];

    public reactionTypes: string[];

    public reactionTimes: string[];

    public serviceLocations: string[];

    public proActives: string[];

    public isGlobalPortfolio: boolean;

    public isMasterPortfolio: boolean;

    public isCorePortfolio: boolean;

    public isValid() {
        let m = this;
        let valid = m.countryId || m.isGlobalPortfolio || m.isMasterPortfolio || m.isCorePortfolio;

        if (valid) {
            valid =
                m.wgs.length +
                m.availabilities.length +
                m.durations.length +
                m.reactionTypes.length +
                m.serviceLocations.length +
                m.proActives.length > 0;
        }

        return valid;
    }
}