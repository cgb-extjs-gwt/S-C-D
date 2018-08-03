import * as React from 'react';
import { FormPanel, RadioField } from '@extjs/ext-react';
import { SelectList, NamedId } from '../../Common/States/CommonStates';

export interface ApprovalFilterActions{
    onInit?()
    onApplicationSelect?(applicationId: string)
}

export interface FilterApprovalProps extends ApprovalFilterActions{
    application: SelectList<NamedId>
}

const filter = (props: FilterApprovalProps) => {
    const radioProps = {
        name: 'application',
        items: [{id: 1, value: "Hardware", checked: true}, 
                {id: 2, value: "Software & Solution", checked: false}]
    }

    const radioFields = radioProps.items.map(item => {
        return <RadioField name={radioProps.name} 
                           key={item.id} 
                           boxLabel = {item.value}
                           value = {item.id.toString()}
                           checked = {item.checked} />

    });
    return (
        <FormPanel shadow
               layout={{type: 'vbox', align: 'left'}}
               flex={1}
               title="Filter By">
               <h3>Application</h3>
               {radioFields}
        </FormPanel>
    );
}

export default filter;