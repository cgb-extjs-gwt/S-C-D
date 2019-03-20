import { NamedId, SortableNamedId } from "../../Common/States/CommonStates";
import { Country } from "../Model/Country";
import { IDictService } from "../Services/IDictService";
import { fakeAvailability } from "./FakeAvailability";
import { fakeCountries } from "./FakeCountries";
import { fakeDuration } from "./FakeDuration";
import { fakePla } from "./FakePla";
import { fakeReactTimeTypes } from "./FakeReactTimeTypes";
import { fakeReactTypes } from "./FakeReactTypes";
import { fakeServiceLocationTypes } from "./FakeServiceLocationTypes";
import { fakeSog } from "./FakeSog";
import { fakeUserCountries } from "./FakeUserCountries";
import { fakeWG } from "./FakeWG";
import { fakeYears } from "./FakeYear";

export class FakeDictService implements IDictService {
    getHardwareWg(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getSwDigitSog(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getCurrencies(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getMasterCountriesNames(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getUserCountryNames(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    public getCountries(): Promise<NamedId[]> {
        return this.fromResult(fakeCountries);
    }

    public getMasterCountries(cache: boolean): Promise<Country[]> {
        throw new Error("Method not implemented.");
    }

    public getUserCountries(cache: boolean): Promise<Country[]> {
        var json = JSON.stringify(fakeUserCountries);
        var data = JSON.parse(json);
        return this.fromResult(data as Country[]);
    }

    public getCountryGroups(): Promise<NamedId<string>[]> {
        return this.getCountries();
    }

    public getCountryGroupDigits(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    public getCountryGroupLuts(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    public getRegions(): Promise<NamedId[]> {
        throw new Error("Method not implemented.");
    }

    public getCountryGroupIsoCode(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    public getCountryQualityGroup(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    public getWG(): Promise<NamedId[]> {
        return this.fromResult(fakeWG);
    }

    public getWgWithMultivendor(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    public getStandardWg(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    public getPla(): Promise<NamedId<string>[]> {
        return this.fromResult(fakePla);
    }

    public getSog(): Promise<NamedId<string>[]> {
        return this.fromResult(fakeSog);
    }

    public getSwDigit(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    public getAvailabilityTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeAvailability);
    }

    public getDurationTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeDuration);
    }

    public getYears(): Promise<NamedId<string>[]> {
        return this.fromResult(fakeYears);
    }

    public getReactionTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeReactTypes);
    }

    public getReactionTimeTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeReactTimeTypes);
    }

    public getServiceLocationTypes(): Promise<SortableNamedId[]> {
        return this.fromResult(fakeServiceLocationTypes);
    }

    public getProActive(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    public getRoles(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    private fromResult<T>(value: T): Promise<T> {
        return Promise.resolve(value);
    }
}
