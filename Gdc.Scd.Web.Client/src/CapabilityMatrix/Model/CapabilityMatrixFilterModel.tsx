export interface CapabilityMatrixFilterModel {
    country?: string;
    wg?: string;
    availability?: string;
    duration?: string;
    reactionType?: string;
    reactionTime?: string;
    serviceLocation?: string;

    isGlobalPortfolio?: boolean;
    isMasterPortfolio?: boolean;
    isCorePortfolio?: boolean;
}