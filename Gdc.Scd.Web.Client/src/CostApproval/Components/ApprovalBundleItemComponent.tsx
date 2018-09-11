import * as React from "react";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { Panel } from "@extjs/ext-react";
import { ApprovalValuesContainerComponent } from "./ApprovalValuesContainerComponent";

Ext.require('Ext.panel.Collapser');

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
        const { id, costBlock } = bundle;

        return (
            <Panel 
                title={this.getTitle()} 
                layout="fit"
                shadow
                collapsed={true}
                collapsible={{
                    direction: 'top',
                    dynamic: true
                }}
                onExpand={this.onPanelExpanded}
                margin="5px 10px 5px 10px"
            >
                {
                    this.state.isFirstExpand &&
                    <ApprovalValuesContainerComponent approvalBundle={bundle} isCheckColumnsVisible={isCheckColumnsVisible}>
                        {children}
                    </ApprovalValuesContainerComponent>
                }
            </Panel>
        );
    }

    private onPanelExpanded = () => {
        if (!this.state.isFirstExpand) {
            this.setState({isFirstExpand: true})
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

        return `
            <div style="float: left; padding-right: 50px;">
                <div>Date: ${date.toDateString()}</div>
                <div>User: ${editUser.name}</div>
                ${
                    regionInput 
                        ? `<div>Region: ${regionInput.name}</div>` 
                        : ''
                }
            </div>
            <div style="float: right;">
                <div>Cost block: ${costBlock.name}</div>
                <div>Cost element: ${costElement.name}</div>
                <div>${editItemCount} ${inputLevel.name}</div>
            </div>
        `;
    }
}