import * as React from 'react';
import { FormPanel, RadioField, CheckBoxField, 
    Panel, Container, DatePickerField,
    Button } from '@extjs/ext-react';
import { SelectList, NamedId, MultiSelectList, ElementWithParent } from '../../Common/States/CommonStates';

export interface ApprovalFilterActions{
    onApplicationSelect?(selectedItemId: string)
    onCostBlockCheck?(selectedItemId: string)
    onCostBlockUncheck?(selectedItemId: string)
    onCostElementCheck?(selectedItemId: string, selectedItemParentId: string)
    onCostElementUncheck?(selectedItemId: string)
    onStartDateChange?(data: Date)
    onEndDateChange?(data: Date)
    onApplyFilter?()
}

export interface FilterApprovalProps extends ApprovalFilterActions{
    application?: SelectList<NamedId>
    costBlocks?: MultiSelectList<NamedId>,
    costElements?: MultiSelectList<ElementWithParent>,
    startDate?: Date,
    endDate?: Date,
}


class Filter extends React.Component<FilterApprovalProps>{

    render(){
        if(this.props.application)
        {
            const applicationRadioFields = this.props.application.list.map(item => {
                return <RadioField name="application" 
                                key={item.id} 
                                boxLabel = {item.name}
                                value = {item.id}
                                checked = {item.id === this.props.application.selectedItemId}
                                onCheck = {() => this.props.onApplicationSelect(item.id)} 
                                />

            });

            const costBlockCheckBoxes = this.props.costBlocks.list.map(item => {
                return <CheckBoxField name="costBlockIds"
                                    key = {item.id}
                                    boxLabel = {item.name}
                                    value = {item.id}
                                    checked = {this.props.costBlocks.selectedItemIds.indexOf(item.id) > -1}
                                    onCheck = {() => this.props.onCostBlockCheck(item.id)}
                                    onUnCheck = {() => this.props.onCostBlockUncheck(item.id)} />
            });


            const costElementsCheckBoxes = (this.props.costElements.list.length) ? this.props.costElements.list.map(item => {
                return <CheckBoxField name="costElementIds"
                                    key = { item.element.id }
                                    boxLabel = {item.element.name}
                                    value = {item.element.id}
                                    checked = {this.props.costElements.selectedItemIds.indexOf(item.element.id) > -1}
                                    onCheck = {() => this.props.onCostElementCheck(item.element.id, item.parentId)}
                                    onUnCheck = {() => this.props.onCostElementUncheck(item.element.id)}
                                    />
            }) : null;

            return (
                    <FormPanel scrollable shadow
                        layout={{type: 'vbox', align: 'left'}}
                        flex={1}
                        title="Filter By">
                        <h3>Application</h3>
                        { applicationRadioFields }
                        <h3>Cost Blocks</h3>
                        <Container layout={{type: 'hbox'}}>
                            <Panel layout={{type: 'vbox', align: 'left'}}>
                                { costBlockCheckBoxes.slice(0, Math.ceil(costBlockCheckBoxes.length / 2)) }
                            </Panel>
                            <Panel margin='0 15px' layout={{type: 'vbox', align: 'left'}}>
                                { costBlockCheckBoxes.slice(Math.ceil(costBlockCheckBoxes.length / 2), costBlockCheckBoxes.length) }
                            </Panel>
                        </Container> 
                        <h3>Cost Elements</h3>
                        {costElementsCheckBoxes ? <Container layout={{type: 'hbox'}}>
                            <Panel layout={{type: 'vbox', align: 'left'}}>
                                { costElementsCheckBoxes.slice(0, Math.ceil(costElementsCheckBoxes.length / 2)) }
                            </Panel>
                            <Panel margin='0 15px' layout={{type: 'vbox', align: 'left'}}>
                                { costElementsCheckBoxes.slice(Math.ceil(costElementsCheckBoxes.length / 2), costElementsCheckBoxes.length) }
                            </Panel>
                        </Container> : <span style={{color: 'red'}}>
                                            Please select Cost Blocks above...
                                        </span>}
                        
                        <h3>Date</h3>
                        <Container layout={{type: 'hbox'}}>
                                <Panel>
                                    <DatePickerField value={this.props.startDate}
                                                    destroyPickerOnHide
                                                    dateFormat = "d.m.Y"
                                                    label = "From"
                                                    picker = {{
                                                        yearFrom: 2018
                                                    }} 
                                                    onChange = {(el, newDate: Date) => this.props.onStartDateChange(newDate)}
                                                    />
                                </Panel >
                                <Panel margin='0 20px'>
                                <DatePickerField value={this.props.endDate}
                                                    destroyPickerOnHide
                                                    label = "To"
                                                    dateFormat = "d.m.Y"
                                                    picker = {{
                                                        yearFrom: 2018
                                                    }} 
                                                    onChange = {(el, newDate: Date) => this.props.onEndDateChange(newDate)}/>
                                </Panel>
                        </Container>
                        <Container margin="20px 0 0 0" layout={{type: 'hbox', align: 'center'}}>
                                        <Button disabled = { costElementsCheckBoxes ? false : true} 
                                        iconCls="x-fa fa-filter" 
                                        text="Filter" ui = "action raised" 
                                        handler = { this.props.onApplyFilter }
                                        />         
                        </Container>
                    </FormPanel>
            );
        }else
        {
            return (
                <Container masked={{xtype: "loadmask", message: "Loading"}}></Container>
            );
        }
    }
}

export default Filter;