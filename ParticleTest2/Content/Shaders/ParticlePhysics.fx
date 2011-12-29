
texture temporaryMap;
sampler temporarySampler : register(s0)  = sampler_state
{
    Texture   = <temporaryMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

texture positionMap;
sampler positionSampler  = sampler_state
{
    Texture   = <positionMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

texture velocityMap;
sampler velocitySampler = sampler_state
{
    Texture   = <velocityMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

texture randomMap;
sampler randomSampler : register(s0) = sampler_state
{
    Texture   = <randomMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = wrap;
    AddressV  = wrap;
};

texture displacementMap;
sampler displacementSampler = sampler_state
{
    Texture   = <displacementMap>;
   MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

float maxLife = 20000.0f;

int screenBorderFromCenterY = 800;
int screenBorderFromCenterX = 1430;

float degredation = 0.1;
bool isDownGravityOn = false;
bool isCoreGravityOn = false;

float3 generateNewPosition(float2 uv)
{
		float4 rand =  tex2D(randomSampler, uv);
		return float3(rand.x*(screenBorderFromCenterX*2),0,rand.y*(screenBorderFromCenterY*2));
}

float4 ResetPositionsPS(in float2 uv : TEXCOORD0) : COLOR
{
	float3 val = generateNewPosition(uv);
	return float4(generateNewPosition(uv), maxLife);
}

int coreGravityStrength = 450000;
float2x2 nintyDegreeRot = { 0.0f, -1.0f,   
                            1.0f, 0.0f 
                          };   
float4 ResetRotateVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 pos = tex2D(positionSampler, uv);
	float2 orthogonalToPos = mul(nintyDegreeRot, float2(pos.x, pos.z));
	float4 finalVector = float4(orthogonalToPos.x, 0, orthogonalToPos.y,0);
	float magnitude = distance(0,pos);
	float4 finalNormalizedVector = (finalVector * (1/magnitude));
	return finalVector/9;
}

float4 ResetVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 newVelocity = float4(0,0,0,0);
	if(isCoreGravityOn)
	{
		newVelocity = ResetRotateVelocitiesPS(uv);
	}
	return newVelocity;
}




float elapsedTime = 1.0f;
float4 UpdatePositionsPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 pos = tex2D(positionSampler, uv);
	if (pos.w >= maxLife)
	{
		// Restart time
		pos.w -= maxLife;
		// Compute new position and direction
		pos.xyz = generateNewPosition(uv);
	} 
	else
	{
		// Update particle position and life
		float4 velocity = tex2D(velocitySampler, uv);
        pos.xyz += elapsedTime * velocity;
        pos.w += elapsedTime;
	}
	return pos;
}

float4 pullLocation;
float pullStrength;

float4 UpdateVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
         float4 velocity = tex2D(velocitySampler, uv);
         float4 pos = tex2D(positionSampler, uv);
         if (pos.w>= maxLife)
         {
                 //reset velocity 
                 velocity = ResetVelocitiesPS(uv);
         }
         else
         {
                //gravitational acceleration. Tweak it until you like the effect :) 
                if(isDownGravityOn)
                {
					velocity.z += 20.0 * elapsedTime;
                }
                if(isCoreGravityOn)
                {
					float4 gravityDirection = (0-pos);
					float4 dist = distance(0,gravityDirection);
					velocity += (gravityDirection/dist) * (coreGravityStrength/pow(dist,2)) * elapsedTime;
                }
                
                 
                 //pull by mouse location (pullStrength is 0 if mouse not pressed)
				float4 pullDirection = (pullLocation - pos);
				float distance = distance(0, pullDirection);
				//assuming unit mass, v1 = v0 + a*t where a=F
				velocity += pullDirection * (pullStrength/pow(distance,1.1)) * elapsedTime; 
				 
         }
         
         if(pos.z < -screenBorderFromCenterY)
         {
			velocity.z = abs(velocity.z*degredation);
         }
         if(pos.z > screenBorderFromCenterY)
         {
			velocity.z = -1*abs(velocity.z*degredation);
         }
         
         if(pos.x < -screenBorderFromCenterX)
         {
			velocity.x = abs(velocity.x*degredation);
         }
         if(pos.x > screenBorderFromCenterX)
         {
			velocity.x = -1*abs(velocity.x*degredation);
         }
         
         return velocity;
}



float4 CopyTexturePS(in float2 uv : TEXCOORD0) : COLOR
{
	return tex2D(temporarySampler,uv);
}

technique CopyTexture
{
    pass P0
    {
        pixelShader  = compile ps_2_0 CopyTexturePS();
    }
}

technique ResetPositions
{
    pass P0
    {
        pixelShader  = compile ps_2_0 ResetPositionsPS();
    }
}
technique ResetVelocities
{
    pass P0
    {
        pixelShader  = compile ps_2_0 ResetVelocitiesPS();
    }
}

technique UpdatePositions
{
    pass P0
    {
        pixelShader  = compile ps_2_0 UpdatePositionsPS();
    }
}

technique UpdateVelocities
{
    pass P0
    {
        pixelShader  = compile ps_2_0 UpdateVelocitiesPS();
    }
}