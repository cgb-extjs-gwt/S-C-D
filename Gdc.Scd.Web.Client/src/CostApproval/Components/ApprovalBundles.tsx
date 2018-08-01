import * as React from 'react';
import { Panel } from '@extjs/ext-react';

const approvalBundles = (props) => {
    return (
        <Panel style={{display: 'inline-block'}} shadow
               title="Bundles to approve"
               flex={2}>
               <p>Hello from Filter Container</p>
        </Panel>
    );
}

export default approvalBundles;