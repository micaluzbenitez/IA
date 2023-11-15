using UnityEngine;

[System.Serializable]
public class Neuron
{
	public float[] weights = null;
	public float bias = 0f;
	public float p = 0f;

	public int WeightsCount
	{
		get { return weights.Length; }
	}

	public Neuron(int weightsCount, float bias, float p)
	{
		weights = new float[weightsCount];

		for (int i = 0; i < weights.Length; i++)
		{
			weights[i] = Random.Range(-1.0f, 1.0f);
		}

		this.bias = bias;
		this.p = p;
	}

	public float Synapsis(float[] input) // Metodo synapsis de una neurona
    {
		// (9) Recibo los inputs y segun la cantidad de inputs, le hago una operacion matematica cualquiera a mi
		// parametro "a" entre el input correspondiente a esa neurona y el peso de una neurona, despues multiplico
		// ese resultado por el bais de la neurona y aplico el metodo Sigmoid() a mi parametro

		float a = 0;

		for (int i = 0; i < input.Length; i++)
		{
			a += weights[i] * input[i];

            // (10) El peso puede ser un valor random o no
            // Si yo no tengo un genoma que aplicarle a este valor, es random
            // Un genoma es un array de floats

            /*
			Los genomas que fueron usados para el resultado son guardados y el genoma de la red neuronal es sobre el que vamos a estar iterando de generación en generación, 
			promediándolo con el genoma de otros agentes que corrieron simultáneamente en la misma simulación. Entonces si cruzo uno que lo hizo mal con uno que lo hizo un 
			poco mejor, va a generar un nuevo valor. 

			Se promedia ya que se quieren generar cruzas genéticas, que es cómo funciona la evolución. De esa cruza se calcula quien lo hizo mejor y quien lo hizo peor, 
			lo que lo hicieron mejor tienen más chances en reproducirse, los que lo hicieron mal quedan descartados, se guardan los mejores de las generaciones anteriores, etc.
			*/
        }

        a += bias * weights[weights.Length - 1];

        // (12) El bais es como un multiplicador de nuestro output, para evitar que el resultado entre el peso y el input sea un numero
		// ridículamente bajo, para que la neurona tienda a dar valores más absolutos
		// El valor del bais es negativo por convención

        return Sigmoid(a);
	}

	public int SetWeights(float[] newWeights, int fromId)
	{
		for (int i = 0; i < weights.Length; i++)
		{
            this.weights[i] = newWeights[i + fromId];
		}

		return fromId + weights.Length;
	}

	public float[] GetWeights()
	{
		return this.weights;
	}

    // (13) Sigmoid es una función que, si tengo un valor con decimales, tiende a acercarlo a su valor real más cercano
	// Es un límite, tiende a llevar un valor a sus límites
    // Esto sirve para que nuestra IA tome decisiones booleanas con menos precisión
    // Lo que hace que la IA se note que es una IA y no es un humano en los videojuegos, ya que toma decisiones más absolutas
    private float Sigmoid(float a)
	{
		return 1.0f / (1.0f + Mathf.Exp(-a / p));
	}
}