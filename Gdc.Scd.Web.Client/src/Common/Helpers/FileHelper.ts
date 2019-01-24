export const getBase64Data = file => new Promise<string>(
    (resolve, reject) => {
        const fileReader = new FileReader();

        fileReader.onload = (event: any) => {
            const fileDataRaw: string = event.target.result;
            const fileData = fileDataRaw.split('base64,')[1];

            resolve(fileData);
        }

        fileReader.onerror = () => reject('file reading error');

        fileReader.readAsDataURL(file);
    }
)