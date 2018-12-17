export interface PortfolioFilterModel {
    country?: string;
    wg?: string;
    availability?: string;
    duration?: string;
    reactionType?: string;
    reactionTime?: string;
    serviceLocation?: string;
    proActive?: string;

    isGlobalPortfolio?: boolean;
    isMasterPortfolio?: boolean;
    isCorePortfolio?: boolean;
}