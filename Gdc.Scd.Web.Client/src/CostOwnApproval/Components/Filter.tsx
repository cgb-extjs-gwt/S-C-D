import * as React from "react";
import { Container, ComboBoxField } from "@extjs/ext-react";
import { NamedId } from "../../Common/States/CommonStates";
import { Store } from "../../Common/States/ExtStates";
import { FilterContainer } from "../../Approval/Components/FilterContainer";
import { OWN_COST_APPROVAL_PAGE } from "../Constants/CostOwnApprovalConstants";
import { ApprovalBundleState } from "../../Approval/States/ApprovalState";

export interface OwnApproveBundlesFilterActions {
    onStateSelect?(state: ApprovalBundleState)
}

export interface OwnApproveBundlesFilterProps extends OwnApproveBundlesFilterActions {
    selectedState?: ApprovalBundleState
}

export class OwnApproveBundlesFilter extends React.Component<OwnApproveBundlesFilterProps> {
    private readonly stateStore: Store<NamedId<ApprovalBundleState.Saved>> = Ext.create('Ext.data.Store', {
        data: [
            { id: ApprovalBundleState.Saved,     name: 'Not sent' },
            { id: ApprovalBundleState.Approving, name: 'Pending'  },
            { id: ApprovalBundleState.Approved,  name: 'Approved' },
            { id: ApprovalBundleState.Rejected,  name: 'Rejected' }
        ] as NamedId<ApprovalBundleState.Saved>[]
    });

    public render() {
        const { selectedState } = this.props;
        const selectionState = this.stateStore.getById(selectedState);

        return (
            <Container layout={{type: 'vbox', align: 'left'}}>
                <FilterContainer pageName={OWN_COST_APPROVAL_PAGE}/>
                <ComboBoxField 
                    label="State" 
                    store={this.stateStore} 
                    selection={selectionState} 
                    onChange={this.onStateChanged}
                    valueField="id"
                    displayField="name"
                    queryMode="local"
                />
            </Container>
        );
    }

    private onStateChanged = (combobox, newValue: ApprovalBundleState, oldValue: ApprovalBundleState) => {
        const { onStateSelect } = this.props;

        onStateSelect && onStateSelect(newValue);
    }
}