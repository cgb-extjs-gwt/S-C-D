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
}

export interface FilterApprovalProps extends ApprovalFilterActions{
    application: SelectList<NamedId>,
    costBlocks: MultiSelectList<NamedId>,
    costElements: MultiSelectList<ElementWithParent>,
    startDate: Date,
    endDate: Date
}

const filter = (props: FilterApprovalProps) => {

    const applicationRadioFields = props.application.list.map(item => {
        return <RadioField name="application" 
                           key={item.id} 
                           boxLabel = {item.name}
                           value = {item.id}
                           checked = {item.id === props.application.selectedItemId}
                           onCheck = {() => props.onApplicationSelect(item.id)} />

    });

    const costBlockCheckBoxes = props.costBlocks.list.map(item => {
        return <CheckBoxField name="costBlockIds"
                              key = {item.id}
                              boxLabel = {item.name}
                              value = {item.id}
                              checked = {props.costBlocks.selectedItemIds.indexOf(item.id) > -1}
                              onCheck = {() => props.onCostBlockCheck(item.id)}
                              onUnCheck = {() => props.onCostBlockUncheck(item.id)} />
    });


    const costElementsCheckBoxes = (props.costElements.list.length) ? props.costElements.list.map(item => {
        return <CheckBoxField name="costElementIds"
                              key = { item.element.id }
                              boxLabel = {item.element.name}
                              value = {item.element.id}
                              checked = {props.costElements.selectedItemIds.indexOf(item.element.id) > -1}
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
                        <DatePickerField value={props.startDate}
                                         destroyPickerOnHide
                                         dateFormat = "d.m.Y"
                                         label = "From"
                                         picker = {{
                                             yearFrom: 2018
                                         }} />
                    </Panel >
                    <Panel margin='0 20px'>
                    <DatePickerField value={props.endDate}
                                         destroyPickerOnHide
                                         label = "To"
                                         dateFormat = "d.m.Y"
                                         picker = {{
                                             yearFrom: 2018
                                         }} />
                    </Panel>
               </Container>
               <Container margin="20px 0 0 0" layout={{type: 'hbox', align: 'center'}}>
                               <Button disabled = { costElementsCheckBoxes ? false : true} 
                               iconCls="x-fa fa-filter" 
                               text="Filter" ui = "action raised" />         
               </Container>
        </FormPanel>
    );
}

export default filter;