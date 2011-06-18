#include "StdAfx.h"

RH_C_FUNCTION ON_Viewport* ON_Viewport_New(const ON_Viewport* pVP)
{
	if (pVP)
	{
		return new ON_Viewport(*pVP);
	}
	return new ON_Viewport();
}

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION ON_Viewport* ON_Viewport_New2(const CRhinoViewport* pRhinoViewport)
{
  if( pRhinoViewport )
    return new ON_Viewport(pRhinoViewport->VP());
  return new ON_Viewport();
}
#endif

RH_C_FUNCTION bool ON_Viewport_GetBool(const ON_Viewport* pConstViewport, int which)
{
  const int idxIsValidCamera = 0;
  const int idxIsValidFrustum = 1;
  const int idxIsValid = 2;
  const int idxIsPerspectiveProjection = 3;
  const int idxIsParallelProjection = 4;
  const int idxIsCameraLocationLocked = 5;
  const int idxIsCameraDirectionLocked = 6;
  const int idxIsCameraUpLocked = 7;
  const int idxIsFrustumLeftRightSymmetric = 8;
  const int idxIsFrustumTopBottomSymmetric = 9;
  bool rc = false;
  if( pConstViewport )
  {
    switch(which)
    {
    case idxIsValidCamera:
      rc = pConstViewport->IsValidCamera();
      break;
    case idxIsValidFrustum:
      rc = pConstViewport->IsValidFrustum();
      break;
    case idxIsValid:
      rc = pConstViewport->IsValid()?true:false;
      break;
#if defined(RHINO_V5SR) // only available in V5
    case idxIsPerspectiveProjection:
      rc = pConstViewport->IsPerspectiveProjection();
      break;
    case idxIsParallelProjection:
      rc = pConstViewport->IsParallelProjection();
      break;
    case idxIsCameraLocationLocked:
      rc = pConstViewport->CameraLocationIsLocked();
      break;
    case idxIsCameraDirectionLocked:
      rc = pConstViewport->CameraDirectionIsLocked();
      break;
    case idxIsCameraUpLocked:
      rc = pConstViewport->CameraUpIsLocked();
      break;
    case idxIsFrustumLeftRightSymmetric:
      rc = pConstViewport->FrustumIsLeftRightSymmetric();
      break;
    case idxIsFrustumTopBottomSymmetric:
      rc = pConstViewport->FrustumIsTopBottomSymmetric();
      break;
#endif
    default:
      break;
    }
  }
  return rc;
}


RH_C_FUNCTION bool ON_Viewport_ChangeToParallelProjection(ON_Viewport* pVP, bool symmetricFrustum)
{
#if defined(RHINO_V5SR) // only available in V5
	return pVP ? pVP->ChangeToParallelProjection(symmetricFrustum) : false;
#else
  return false;
#endif
}

RH_C_FUNCTION bool ON_Viewport_ChangeToPerspectiveProjection(ON_Viewport* pVP, double targetDistance, bool symmetricFrustum, double lensLength)
{
#if defined(RHINO_V5SR) // only available in V5
	return pVP ? pVP->ChangeToPerspectiveProjection(targetDistance, symmetricFrustum, lensLength) : false;
#else
  return false;
#endif
}

RH_C_FUNCTION bool ON_Viewport_ChangeToTwoPointPerspectiveProjection(ON_Viewport* pVP, double targetDistance, ON_3DVECTOR_STRUCT up, double lensLength)
{
#if defined(RHINO_V5SR) // only available in V5
	return pVP ? pVP->ChangeToTwoPointPerspectiveProjection(targetDistance, ON_3dVector(up.val), lensLength) : false;
#else
  return false;
#endif
}

RH_C_FUNCTION void ON_Viewport_CameraLocation(const ON_Viewport* pVP, ON_3dPoint* p)
{
	if (pVP && p)	{ *p = pVP->CameraLocation(); }
}

RH_C_FUNCTION void ON_Viewport_CameraDirection(const ON_Viewport* pVP, ON_3dVector* p)
{
	if (pVP && p)	{ *p = pVP->CameraDirection(); }
}

RH_C_FUNCTION bool ON_Viewport_SetCameraDirection(ON_Viewport* pVP, ON_3DVECTOR_STRUCT v)
{
	return pVP ? pVP->SetCameraDirection(ON_3dVector(v.val)) : false;
}

RH_C_FUNCTION bool ON_Viewport_SetCameraLocation(ON_Viewport* pVP, ON_3DPOINT_STRUCT v)
{
	return pVP ? pVP->SetCameraLocation(ON_3dVector(v.val)) : false;
}

RH_C_FUNCTION void ON_Viewport_CameraUp(const ON_Viewport* pVP, ON_3dVector* p)
{
	if (pVP && p)	{ *p = pVP->CameraUp(); }
}

RH_C_FUNCTION bool ON_Viewport_SetCameraUp(ON_Viewport* pVP, ON_3DVECTOR_STRUCT v)
{
	return pVP ? pVP->SetCameraUp(ON_3dVector(v.val)) : false;
}

RH_C_FUNCTION void ON_Viewport_SetLocked(ON_Viewport* pViewport, int which, bool b)
{
  const int idxCameraLocationLock = 0;
  const int idxCameraDirectionLock = 1;
  const int idxCameraUpLock = 2;
#if defined(RHINO_V5SR) // only available in V5
  if( pViewport )
  {
    if( idxCameraLocationLock == which )
      pViewport->SetCameraLocationLock(b);
    else if( idxCameraDirectionLock == which )
      pViewport->SetCameraDirectionLock(b);
    else if( idxCameraUpLock == which )
      pViewport->SetCameraUpLock(b);
  }
#endif
}


RH_C_FUNCTION void ON_Viewport_SetIsFrustumSymmetry(ON_Viewport* pViewport, bool leftright, bool b)
{
  if( pViewport )
  {
#if defined(RHINO_V5SR) // only available in V5
    if( leftright )
      pViewport->SetFrustumLeftRightSymmetry(b);
    else
      pViewport->SetFrustumTopBottomSymmetry(b);
#endif
  }
}

RH_C_FUNCTION void ON_Viewport_Unlock(ON_Viewport* pViewport, bool camera)
{
  if( pViewport )
  {
#if defined(RHINO_V5SR) // only available in V5
    if( camera )
      pViewport->UnlockCamera();
    else
      pViewport->UnlockFrustumSymmetry();
#endif
  }
}

RH_C_FUNCTION bool ON_Viewport_GetCameraFrame(const ON_Viewport* pVP, ON_3dPoint* location, ON_3dVector* cameraX, ON_3dVector* cameraY, ON_3dVector* cameraZ)
{
  bool rc = false;
	if (pVP && location && cameraX && cameraY && cameraZ)
	{
		rc = pVP->GetCameraFrame(&location->x, &cameraX->x, &cameraY->x, &cameraZ->x);
	}
	return rc;
}

RH_C_FUNCTION void ON_Viewport_CameraAxis(const ON_Viewport* pConstViewport, int iAxis, ON_3dVector* v)
{
	if (pConstViewport && v)
	{
		switch (iAxis)
		{
		case 0:
			*v = pConstViewport->CameraX();
			break;
		case 1:
			*v = pConstViewport->CameraY();
			break;
		case 2:
			*v = pConstViewport->CameraZ();
			break;
		}
	}
}


RH_C_FUNCTION bool ON_Viewport_SetFrustum(ON_Viewport* pViewport, double left, double right, double bottom, double top, double nearDistance, double farDistance)
{
  bool rc = false;
	if (pViewport)
		rc = pViewport->SetFrustum(left, right, bottom, top, nearDistance, farDistance);
	return rc;
}

RH_C_FUNCTION bool ON_Viewport_GetFrustum(const ON_Viewport* pConstViewport, double* left, double* right, double* bottom, double* top, double* nearDistance, double* farDistance)
{
  bool rc = false;
	if (pConstViewport && left && right && bottom && top && nearDistance && farDistance)
	{
		rc =  pConstViewport->GetFrustum(left, right, bottom, top, nearDistance, farDistance);
	}
	return rc;
}

RH_C_FUNCTION bool ON_Viewport_GetFrustrumAspect(const ON_Viewport* pVP, double* dAspect)
{
	if (pVP && dAspect)
		return pVP->GetFrustumAspect(*dAspect);
	return false;
}

RH_C_FUNCTION int ON_Viewport_SetFrustumAspect(ON_Viewport* pVP, double d)
{
	if (pVP)
	{
		return pVP->SetFrustumAspect(d) ? 1:0;
	}
	return 0;
}

RH_C_FUNCTION int ON_Viewport_GetFrustumCenter(const ON_Viewport* pVP, ON_3dPoint* p)
{
	if (pVP && p)
	{
		return pVP->GetFrustumCenter(*p) ? 1:0;
	}
	return 0;
}

RH_C_FUNCTION double ON_Viewport_GetDouble(const ON_Viewport* pConstViewport, int which)
{
  const int idxFrustumLeft = 0;
  const int idxFrustumRight = 1;
  const int idxFrustumBottom = 2;
  const int idxFrustumTop = 3;
  const int idxFrustumNear = 4;
  const int idxFrustumFar = 5;
  const int idxFrustumMinimumDiameter = 6;
  const int idxFrustumMaximumDiameter = 7;
  double rc = 0;
  if( pConstViewport )
  {
    switch(which)
    {
    case idxFrustumLeft:
      rc = pConstViewport->FrustumLeft();
      break;
    case idxFrustumRight:
      rc = pConstViewport->FrustumRight();
      break;
    case idxFrustumBottom:
      rc = pConstViewport->FrustumBottom();
      break;
    case idxFrustumTop:
      rc = pConstViewport->FrustumTop();
      break;
    case idxFrustumNear:
      rc = pConstViewport->FrustumNear();
      break;
    case idxFrustumFar:
      rc = pConstViewport->FrustumFar();
      break;
#if defined(RHINO_V5SR) // only available in V5
    case idxFrustumMinimumDiameter:
      rc = pConstViewport->FrustumMinimumDiameter();
      break;
    case idxFrustumMaximumDiameter:
      rc = pConstViewport->FrustumMaximumDiameter();
      break;
#endif
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Viewport_SetFrustumNearFarBoundingBox(ON_Viewport* pVP, ON_3DPOINT_STRUCT min, ON_3DPOINT_STRUCT max)
{
	if (pVP)
		return pVP->SetFrustumNearFar(min.val, max.val);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_SetFrustumNearFarSphere(ON_Viewport* pVP, ON_3DPOINT_STRUCT center, double radius)
{
	if (pVP)
  	return pVP->SetFrustumNearFar(center.val, radius);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_SetFrustumNearFar(ON_Viewport* pVP, double nearDistance, double farDistance)
{
	if (pVP)
		return pVP->SetFrustumNearFar(nearDistance, farDistance);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_ChangeToSymmetricFrustum(ON_Viewport* pVP, bool isLeftRightSymmetric, bool isTopBottomSymmetric, double targetDistance)
{
	if (pVP)
	{
#if defined(RHINO_V5SR) // only available in V5
		return pVP->ChangeToSymmetricFrustum(isLeftRightSymmetric, isTopBottomSymmetric, targetDistance);
#endif
	}
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetPointDepth(const ON_Viewport* pVP, 
											ON_3DPOINT_STRUCT point, 
											double* nearDistance, 
											double* farDistance, 
											bool growNearFar)
{
	if (pVP && nearDistance && farDistance)
		return pVP->GetPointDepth(point.val, nearDistance, farDistance, growNearFar);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetBoundingBoxDepth(const ON_Viewport* pVP, 
												   ON_3DPOINT_STRUCT min, 
												   ON_3DPOINT_STRUCT max, 
												   double* nearDistance, 
												   double* farDistance, 
												   bool growNearFar)
{
	if (pVP && nearDistance && farDistance)
		return pVP->GetBoundingBoxDepth(ON_BoundingBox(min.val, max.val), nearDistance, farDistance, growNearFar);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetSphereDepth(const ON_Viewport* pVP, 
											 ON_3DPOINT_STRUCT center, 
											 double radius, 
											 double* nearDistance,
											 double* farDistance, 
											 bool growNearFar)
{
	if (pVP && nearDistance && farDistance)
		return pVP->GetSphereDepth(ON_Sphere(center.val, radius), nearDistance, farDistance, growNearFar);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_SetFrustrumNearFar(ON_Viewport* pVP,
												 double nearDistance, 
												 double farDistance, 
												 double minNearDistance, 
												 double minNearOverFar, 
												 double targetDistance)
{
	if (pVP)
		return pVP->SetFrustumNearFar(nearDistance, farDistance, minNearDistance, minNearOverFar, targetDistance);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetPlane(const ON_Viewport* pConstViewport, int which, ON_PLANE_STRUCT* plane)
{
  const int idxNearPlane = 0;
  const int idxFarPlane = 1;
  const int idxLeftPlane = 2;
  const int idxRightPlane = 3;
  const int idxBottomPlane = 4;
  const int idxTopPlane = 5;
  bool rc = false;
	if (pConstViewport && plane)
	{
		ON_Plane _plane;
    switch(which)
    {
    case idxNearPlane:
      rc = pConstViewport->GetNearPlane(_plane);
      break;
    case idxFarPlane:
      rc = pConstViewport->GetFarPlane(_plane);
      break;
    case idxLeftPlane:
      rc = pConstViewport->GetFrustumLeftPlane(_plane);
      break;
    case idxRightPlane:
      rc = pConstViewport->GetFrustumRightPlane(_plane);
      break;
    case idxBottomPlane:
      rc = pConstViewport->GetFrustumBottomPlane(_plane);
      break;
    case idxTopPlane:
      rc = pConstViewport->GetFrustumTopPlane(_plane);
      break;
    default:
      break;
    }
    if( rc )
      CopyToPlaneStruct(*plane, _plane);
	}
	return rc;
}


RH_C_FUNCTION bool ON_Viewport_GetNearFarRect(const ON_Viewport* pConstViewport, bool _near,
										   ON_3dPoint* leftBottom, ON_3dPoint* rightBottom, ON_3dPoint* leftTop, ON_3dPoint* rightTop)
{
  bool rc = false;
	if (pConstViewport && leftBottom && rightBottom && leftTop && leftBottom)
	{
    if( _near )
      rc = pConstViewport->GetNearRect(*leftBottom, *rightBottom, *leftTop, *rightTop);
    else
      rc = pConstViewport->GetFarRect(*leftBottom, *rightBottom, *leftTop, *rightTop);
	}
	return rc;
}

RH_C_FUNCTION bool ON_Viewport_SetScreenPort(ON_Viewport* pVP, int left, int right, int bottom, int top, int _near, int _far)
{
	if (pVP)
		return pVP->SetScreenPort(left, right, bottom, top, _near, _far);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetScreenPort(const ON_Viewport* pConstViewport, int* left, int* right, int* bottom, int* top, int* _near, int* _far)
{
	if (pConstViewport && left && right && bottom && top && _near && _far)
		return pConstViewport->GetScreenPort(left, right, bottom, top, _near, _far);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetScreenPortAspect(const ON_Viewport* pConstViewport, double* dAspect)
{
	if (pConstViewport && dAspect)
		return pConstViewport->GetScreenPortAspect(*dAspect);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetCameraAngle2(const ON_Viewport* pConstViewport, double* halfDiagonalAngle, double* halfVerticalAngle, double* halfHorizontalAngle)
{
	if (pConstViewport && halfDiagonalAngle && halfVerticalAngle && halfHorizontalAngle)
		return pConstViewport->GetCameraAngle(halfDiagonalAngle, halfVerticalAngle, halfHorizontalAngle);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetCameraAngle(const ON_Viewport* pConstViewport, double* d)
{
	if (pConstViewport && d)
		return pConstViewport->GetCameraAngle(d);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_SetCameraAngle(ON_Viewport* pVP, double d)
{
	if (pVP)
		return pVP->SetCameraAngle(d);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetCamera35mmLensLength(const ON_Viewport* pConstViewport, double* d)
{
	if (pConstViewport && d)
	{
#if defined(RHINO_V5SR) // only available in V5
		return pConstViewport->GetCamera35mmLensLength(d);
#endif
	}
	return false;
}

RH_C_FUNCTION bool ON_Viewport_SetCamera35mmLensLength(ON_Viewport* pVP, double d)
{
	if (pVP)
	{
#if defined(RHINO_V5SR) // only available in V5
		return pVP->SetCamera35mmLensLength(d);
#endif
	}
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetXform(const ON_Viewport* pConstViewport, int sourceCoordSystem, int destinationCoordSystem, ON_Xform* matrix)
{
	if (pConstViewport && matrix)
	{
		return pConstViewport->GetXform((ON::coordinate_system)sourceCoordSystem, (ON::coordinate_system)destinationCoordSystem, *matrix);
	}
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetFrustumLine(const ON_Viewport* pConstViewport, double screenX, double screenY, ON_Line* line)
{
	if (pConstViewport && line)
		return pConstViewport->GetFrustumLine(screenX, screenY, *line);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetWorldToScreenScale(const ON_Viewport* pConstViewport, ON_3DPOINT_STRUCT pointInFrustum, double* pixels_per_unit)
{
	if (pConstViewport && pixels_per_unit)
		return pConstViewport->GetWorldToScreenScale(pointInFrustum.val, pixels_per_unit);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_ExtentsBBox(ON_Viewport* pVP, double halfViewAngle, ON_3DPOINT_STRUCT min, ON_3DPOINT_STRUCT max)
{
	if (pVP)
		return pVP->Extents(halfViewAngle, ON_BoundingBox(min.val, max.val));
	return false;
}

RH_C_FUNCTION bool ON_Viewport_ExtentsSphere(ON_Viewport* pVP, double halfViewAngle, ON_3DPOINT_STRUCT center, double radius)
{
	if (pVP)
		return pVP->Extents(halfViewAngle, center.val, radius);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_ZoomToScreenRect(ON_Viewport* pVP, int left, int top, int right, int bottom)
{
	if (pVP)
		return pVP->ZoomToScreenRect(left, top, right, bottom);
	return false;

}

RH_C_FUNCTION bool ON_Viewport_DollyCamera(ON_Viewport* pVP, ON_3DVECTOR_STRUCT dollyVector)
{
	if (pVP)
		return pVP->DollyCamera(dollyVector.val);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_GetDollyCameraVector(const ON_Viewport* pConstViewport, 
													int screenX0, 
													int screenY0, 
													int screenX1, 
													int screenY1, 
													double projectionPlaneDistance, 
													ON_3dVector* v)
{
	if (pConstViewport && v)
		return pConstViewport->GetDollyCameraVector(screenX0, screenY0, screenX1, screenY1, projectionPlaneDistance, *v);
	return false;
}

RH_C_FUNCTION bool ON_Viewport_DollyFrustum(ON_Viewport* pVP, double dollyDistance)
{
	if (pVP)
		return pVP->DollyFrustum(dollyDistance);
	return false;
}

RH_C_FUNCTION void ON_Viewport_GetViewScale(const ON_Viewport* pConstViewport, double* w, double* h)
{
	if (pConstViewport && w && h)
		pConstViewport->GetViewScale(w,h);
}

RH_C_FUNCTION void ON_Viewport_SetViewScale(ON_Viewport* pVP, double w, double h)
{
	if (pVP)
		pVP->SetViewScale(w,h);
}

RH_C_FUNCTION void ON_Viewport_ClipModXform(const ON_Viewport* pConstViewport, ON_Xform* matrix)
{
	if (pConstViewport && matrix)
		*matrix = pConstViewport->ClipModXform();
}

RH_C_FUNCTION void ON_Viewport_ClipModInverseXform(const ON_Viewport* pConstViewport, ON_Xform* matrix)
{
	if (pConstViewport && matrix)
		*matrix = pConstViewport->ClipModInverseXform();
}

RH_C_FUNCTION bool ON_Viewport_ClipModXformIsIdentity(const ON_Viewport* pConstViewport)
{
	return pConstViewport ? pConstViewport->ClipModXformIsIdentity(): false;
}

RH_C_FUNCTION void ON_Viewport_FrustumCenterPoint(const ON_Viewport* pConstViewport, double targetDistance, ON_3dPoint* point)
{
	if (pConstViewport && point)
	{
#if defined(RHINO_V5SR) // only available in V5
		*point = pConstViewport->FrustumCenterPoint(targetDistance);
#endif
	}
}

RH_C_FUNCTION void ON_Viewport_TargetPoint(const ON_Viewport* pConstViewport, ON_3dPoint* point)
{
	if (pConstViewport && point)
		*point = pConstViewport->TargetPoint();
}

RH_C_FUNCTION void ON_Viewport_SetTargetPoint(ON_Viewport* pVP, ON_3DPOINT_STRUCT point)
{
	if (pVP)
		pVP->SetTargetPoint(point.val);
}

RH_C_FUNCTION double ON_Viewport_TargetDistance(const ON_Viewport* pConstViewport, bool useFrustumCenterFallback)
{
	if (pConstViewport)
	{
#if defined(RHINO_V5SR) // only available in V5
		return pConstViewport->TargetDistance(useFrustumCenterFallback);
#endif
	}
	return 0.0;
}

RH_C_FUNCTION void ON_Viewport_GetPerspectiveClippingPlaneConstraints(ON_3DPOINT_STRUCT cameraLocation, 
																	  int depthBufferBitDepth, double* minNearDist, double* minNearOverFar)
{
	if (minNearDist && minNearOverFar)
	{
#if defined(RHINO_V5SR) // only available in V5
		ON_Viewport::GetPerspectiveClippingPlaneConstraints(cameraLocation.val, (unsigned int)depthBufferBitDepth, minNearDist, minNearOverFar);
#endif
	}
}

RH_C_FUNCTION void ON_Viewport_SetPerspectiveClippingPlaneConstraints(ON_Viewport* pVP, int depthBufferBitDepth)
{
	if (pVP)
	{
#if defined(RHINO_V5SR) // only available in V5
		pVP->SetPerspectiveClippingPlaneConstraints((unsigned int)depthBufferBitDepth);
#endif
	}
}

RH_C_FUNCTION double ON_Viewport_GetPerspectiveMinNearOverFar(const ON_Viewport* pVP)
{
	if (pVP)
	{
		return pVP->PerspectiveMinNearOverFar();
	}
	return 0.0;
}

RH_C_FUNCTION int ON_Viewport_SetPerspectiveMinNearOverFar(ON_Viewport* pVP, double d)
{
	if (pVP)
	{
		pVP->SetPerspectiveMinNearOverFar(d);
		return 1;
	}
	return 0;
}

RH_C_FUNCTION double ON_Viewport_GetPerspectiveMinNearDist(const ON_Viewport* pVP)
{
	if (pVP)
	{
		return pVP->PerspectiveMinNearDist();
	}
	return 0.0;
}

RH_C_FUNCTION int ON_Viewport_SetPerspectiveMinNearDist(ON_Viewport* pVP, double d)
{
	if (pVP)
	{
		pVP->SetPerspectiveMinNearDist(d);
		return 1;
	}
	return 0;
}

RH_C_FUNCTION ON_UUID ON_Viewport_GetViewportId(const ON_Viewport* pVP)
{
	if (pVP)
		return pVP->ViewportId();
	return ON_nil_uuid;
}

RH_C_FUNCTION int ON_Viewport_SetViewportId(ON_Viewport* pVP, ON_UUID id)
{
	if (pVP)
	{
		pVP->SetViewportId(id);
		return 1;
	}
	return 0;

}

RH_C_FUNCTION void ON_Viewport_Delete(ON_Viewport* pVP)
{
	delete pVP;
}

