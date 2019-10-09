import * as React from "react";
import { BundleListLayout } from "../../Approval/Components/BundleListLayout";
import { BundleView } from "../../Approval/Components/BundleView";
import { OwnApproveRejectContainerComponent } from "./OwnApproveRejectContainer";
import { Bundle, ApprovalBundleState } from "../../Approval/States/ApprovalState";
import { FilterContainer } from "../../Approval/Components/FilterContainer";
import { OWN_COST_APPROVAL_PAGE } from "../Constants/CostOwnApprovalConstants";

export class OwnApprovalLayout extends BundleListLayout {
    protected bundleItemRender(bundle: Bundle) {
        const { onRefresh } = this.props;
        
        return (
            <BundleView 
                key={bundle.id}
                bundle={bundle}
                isCheckColumnsVisible={false}
            >
                {
                    bundle.state == ApprovalBundleState.Saved 
                        ? <OwnApproveRejectContainerComponent 
                            bundleId={bundle.id} 
                            costBlockId={bundle.costBlock.id} 
                            costElementId={bundle.costElement.id}
                            onHandled={onRefresh}
                        />
                        : <div/>
                }
            </BundleView>
        );
    }

    protected filterRender() {
        return (
            <FilterContainer pageName={OWN_COST_APPROVAL_PAGE} isVisibleNotSentState={true}/>
        );
    }
}