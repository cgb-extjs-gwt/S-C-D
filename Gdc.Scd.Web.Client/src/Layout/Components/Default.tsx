import * as React from "react";
import { Container } from "@extjs/ext-react";

const Default = (props) => {
    return (
        <Container margin="10 0 0 10">
            <h1>WELCOME TO SCD 2.0</h1>
            <div>Please use left side bar for navigation.</div>
            <hr />
            <div>Please address all SCD issues to <a href="mailto:SCD_Admin@ts.fujitsu.com">administrators</a></div>
        </Container>
    );
}

export default Default;