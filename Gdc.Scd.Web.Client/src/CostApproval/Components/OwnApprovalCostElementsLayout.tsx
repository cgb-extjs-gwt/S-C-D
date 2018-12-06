import * as React from 'react';
import BaseBundleLayout from "./BaseBundleLayout";
import { ApprovalBundleState } from "../States/ApprovalBundleState";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { OWN_COST_APPROVAL_PAGE } from "../Constants/CostApprovalConstants";
import { OwnApproveRejectContainerComponent } from "./OwnApproveRejectContainerComponent"
import { loadOwnBundlesByFilter } from '../Actions/BundleListActions';

export class OwnApprovalCostElementsLayout extends BaseBundleLayout {
    protected isCheckColumnsVisible(): boolean {
        return false;
    }

    protected getTitle(): string {
        return 'Own Cost Elements Approval';
    }

    protected getPageName(): string {
        return OWN_COST_APPROVAL_PAGE;
    }

    protected getApprovalBundleState(): ApprovalBundleState {
        return ApprovalBundleState.Saved;
    }

    protected buildChildrenBundleItem(bundle: ApprovalBundle, onHandled: () => void) {
        return (
            <OwnApproveRejectContainerComponent 
                bundleId={bundle.id} 
                costBlockId={bundle.costBlock.id} 
                costElementId={bundle.costElement.id}
                onHandled={onHandled}
            />
        );
    }

    protected buildReloadBundlesAction(pageName: string, approvalBundleState: ApprovalBundleState) {
        return loadOwnBundlesByFilter(pageName, approvalBundleState);
    }
}