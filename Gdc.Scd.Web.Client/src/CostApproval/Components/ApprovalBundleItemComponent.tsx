import * as React from "react";
import { Accordion } from "../../Common/Components/Accordion";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { ApprovalValuesContainerComponent } from "./ApprovalValuesContainerComponent";

export interface ApprovalBundleItemProps {
    bundle: ApprovalBundle
    isCheckColumnsVisible: boolean
}

interface ApprovalBundleItemState {
    isFirstExpand: boolean
}

export class ApprovalBundleItemComponent extends React.Component<ApprovalBundleItemProps, ApprovalBundleItemState> {
    constructor(props) {
        super(props);

        this.state = {
            isFirstExpand: false
        }
    }

    public render() {
        const { bundle, children, isCheckColumnsVisible } = this.props;

        let approvalContainer = null;
        if (this.state.isFirstExpand) {
            approvalContainer = <ApprovalValuesContainerComponent approvalBundle={bundle} isCheckColumnsVisible={isCheckColumnsVisible}>
                {children}
            </ApprovalValuesContainerComponent>;
        }

        return <Accordion title={this.getTitle()} onExpand={this.onPanelExpanded}>
            {approvalContainer}
        </Accordion>;
    }

    private onPanelExpanded = () => {
        if (!this.state.isFirstExpand) {
            this.setState({ isFirstExpand: true })
        }
    }

    private getTitle() {
        const {
            editDate,
            editUser,
            regionInput,
            costElement,
            editItemCount,
            inputLevel,
            costBlock
        } = this.props.bundle;

        const date = new Date(editDate);

        let region = null;

        if (regionInput) {
            region = <div>Region: {regionInput.name}</div>;
        }

        return (
            <div className="approval-panel-title">
                <div>
                    <div>Date: {date.toDateString()}</div>
                    <div>User: {editUser.name}</div>
                    {region}
                </div>
                <div>
                    <div>Cost block: {costBlock.name}</div>
                    <div>Cost element: {costElement.name}</div>
                    <div>{editItemCount} {inputLevel.name}</div>
                </div>
            </div>
        );
    }
}