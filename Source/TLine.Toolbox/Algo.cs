using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace TripLine.Toolbox
{
    public class Algo
    {
        /// <summary>
        /// Computes the FFT on the provided arrary. A Hamming window is applied on the samples before processing
        /// </summary>
        /// <param name="IsamplesArray"></param>
        /// <param name="QsamplesArray"></param>
        /// <param name="normaliseMagnitude"></param>
        /// <param name="shiftFFT"></param>
        /// <returns></returns>
        public static List<double> ComputeFft(Int16[] IsamplesArray, Int16[] QsamplesArray, bool normaliseMagnitude, bool shiftFFT)
        {
            if (IsamplesArray.Count() == 0 || QsamplesArray.Count() == 0)
                return new List<double>();

            var noOfSamplesPerArray = IsamplesArray.Length;
            var smoothSamplesI = new double[noOfSamplesPerArray];
            var smoothSamplesQ = new double[noOfSamplesPerArray];
            var smoothingWindow = MathNet.Numerics.Window.Hamming(noOfSamplesPerArray);

            //Apply Hamming smoothing window
            for (var i = 0; i < noOfSamplesPerArray; i++)
            {
                smoothSamplesI[i] = smoothingWindow[i] * IsamplesArray[i];
                smoothSamplesQ[i] = smoothingWindow[i] * QsamplesArray[i];
            }

            var arrComplex = new Complex[noOfSamplesPerArray];
            for (var i = 0; i < noOfSamplesPerArray; i++)
            {
                arrComplex[i] = new Complex(smoothSamplesI[i], smoothSamplesQ[i]);
            }

            Fourier.Forward(arrComplex, normaliseMagnitude ? FourierOptions.Matlab : FourierOptions.Default);

            if (shiftFFT)
            {
                var fftShifted = ShiftFFT(arrComplex);
                return fftShifted.Select(c => c.Magnitude).ToList();
            }
            return arrComplex.Select(c => c.Magnitude).ToList();
        }

        // TODO: Expose an overload that takes a RawSampleCollection
         
        public static List<Complex> ShiftFFT(Complex[] dataArray)
        {
            var middleIndex = dataArray.Length / 2;

            var shiftedArray = new List<Complex>();
            for (int i = 0; i < dataArray.Length / 2; i++)
            {
                shiftedArray.Add(dataArray[middleIndex + i]);
            }

            for (int i = 0; i < dataArray.Length / 2; i++)
            {
                shiftedArray.Add(dataArray[i]);
            }

            return shiftedArray;
        }

    }
}