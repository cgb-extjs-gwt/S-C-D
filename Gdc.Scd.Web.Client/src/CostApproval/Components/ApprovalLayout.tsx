import * as React from "react";
import { COST_APPROVAL_PAGE } from "../Constants/CostApprovalConstants";
import { BundleListLayout } from "../../Approval/Components/BundleListLayout";
import { BundleView } from "../../Approval/Components/BundleView";
import { ApproveRejectContainerComponent } from "./ApproveRejectContainer";
import { Bundle } from "../../Approval/States/ApprovalState";
import { FilterContainer } from "../../Approval/Components/FilterContainer";

export class ApprovalLayout extends BundleListLayout {
    protected bundleItemRender(bundle: Bundle) {
        const { onRefresh, isCheckColumnsVisible } = this.props;

        return (
            <BundleView 
                key={bundle.id}
                bundle={bundle}
                isCheckColumnsVisible={isCheckColumnsVisible()}
            >
                <ApproveRejectContainerComponent bundleId={bundle.id} onHandled={onRefresh}/>
            </BundleView>
        );
    }

    protected filterRender() {
        return (
            <FilterContainer pageName={COST_APPROVAL_PAGE} isVisibleNotSentState={false}/>
        );
    }
}