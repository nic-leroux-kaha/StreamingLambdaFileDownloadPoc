import { readFile } from 'fs/promises';
import { Readable } from 'stream';
import { pipeline } from 'stream/promises';

export const handler = awslambda.streamifyResponse(async (event, responseStream) => {
  try {
    // Set the content type to PDF
    responseStream.setContentType('application/pdf');

    // Read the file
    const pdfBuffer = await readFile('./files/mypdf.pdf');

    // Create a readable stream from the buffer
    const readableStream = Readable.from(pdfBuffer);

    // Stream the file content to the response
    await pipeline(readableStream, responseStream);
  } catch (error) {
    console.error('Error streaming file:', error);
    responseStream.write('Error streaming file');
    responseStream.end();
  }
});
