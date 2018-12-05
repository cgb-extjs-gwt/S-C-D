import * as React from 'react';
import { FilterBundleContainer } from './FilterBundlesContainer';
import { Container } from '@extjs/ext-react';
import { ApprovalBundleListContainerComponent } from './ApprovalBundleListContainerComponent';
import BaseBundleLayout from './BaseBundleLayout';
import { ApprovalBundle } from '../States/ApprovalBundle';
import { ApproveRejectContainerComponent } from './ApproveRejectContainerComponent';
import { ApprovalBundleState } from '../States/ApprovalBundleState';
import { COST_APPROVAL_PAGE } from '../Constants/CostApprovalConstants';
import { loadBundlesByFilter } from '../Actions/BundleListActions';

export class ApprovalCostElementsLayout extends BaseBundleLayout {
    protected isCheckColumnsVisible(): boolean {
        return true;
    }

    protected getTitle(): string {
        return 'Cost Elements Approval';
    }

    protected getPageName(): string {
        return COST_APPROVAL_PAGE;
    }
    
    protected getApprovalBundleState(): ApprovalBundleState {
        return ApprovalBundleState.Approving;
    }

    protected buildChildrenBundleItem(bundle: ApprovalBundle, onHandled: () => void) {
        return (
            <ApproveRejectContainerComponent bundleId={bundle.id} onHandled={onHandled} />
        );
    }

    protected buildReloadBundlesAction(pageName: string, approvalBundleState: ApprovalBundleState) {
        return loadBundlesByFilter(pageName, approvalBundleState);
    }
}

export default ApprovalCostElementsLayout;